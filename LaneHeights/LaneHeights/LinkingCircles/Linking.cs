using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace LaneHeights_LinkingCircles
{
    /// <summary>
    /// Utility class to manage and save links
    /// between objects
    /// </summary>
    public class LinkedObjectManager
    {
        const string kCompanyDict = "AsdkLinks";
        const string kApplicationDict = "AsdkLinkedObjects";
        const string kXrecPrefix = "LINKXREC";

        Dictionary<ObjectId, ObjectIdCollection> m_dict;

        // Constructor
        public LinkedObjectManager()
        {
            m_dict = new Dictionary<ObjectId, ObjectIdCollection>();
        }

        // Create a bi-directional link between two objects
        public void LinkObjects(ObjectId from, ObjectId to)
        {
            CreateLink(from, to);
            CreateLink(to, from);
        }

        // Helper function to create a one-way
        // link between objects
        private void CreateLink(ObjectId from, ObjectId to)
        {
            ObjectIdCollection existingList;
            if (m_dict.TryGetValue(from, out existingList))
            {
                if (!existingList.Contains(to))
                {
                    existingList.Add(to);
                    m_dict.Remove(from);
                    m_dict.Add(from, existingList);
                }
            }
            else
            {
                ObjectIdCollection newList = new ObjectIdCollection();
                newList.Add(to);
                m_dict.Add(from, newList);
            }
        }

        // Remove bi-directional links from an object
        public void RemoveLinks(ObjectId from)
        {
            ObjectIdCollection existingList;
            if (m_dict.TryGetValue(from, out existingList))
            {
                m_dict.Remove(from);
                foreach (ObjectId id in existingList)
                {
                    RemoveFromList(id, from);
                }
            }
        }

        // Helper function to remove an object reference
        // from a list (assumes the overall list should
        // remain)
        private void RemoveFromList(ObjectId key, ObjectId toremove)
        {
            ObjectIdCollection existingList;
            if (m_dict.TryGetValue(key, out existingList))
            {
                if (existingList.Contains(toremove))
                {
                    existingList.Remove(toremove);
                    m_dict.Remove(key);
                    m_dict.Add(key, existingList);
                }
            }
        }

        // Return the list of objects linked to
        // the one passed in
        public ObjectIdCollection GetLinkedObjects(ObjectId from)
        {
            ObjectIdCollection existingList;
            m_dict.TryGetValue(from, out existingList);
            return existingList;
        }

        // Check whether the dictionary contains
        // a particular key
        public bool Contains(ObjectId key)
        {
            return m_dict.ContainsKey(key);
        }

        // Save the link information to a special
        // dictionary in the database
        public void SaveToDatabase(Database db)
        {
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                ObjectId dictId = GetLinkDictionaryId(db, true);
                DBDictionary dict = (DBDictionary)tr.GetObject(dictId, OpenMode.ForWrite);
                int xrecCount = 0;

                foreach (KeyValuePair<ObjectId, ObjectIdCollection> kv in m_dict)
                {
                    // Prepare the result buffer with our data
                    ResultBuffer rb = new ResultBuffer(new TypedValue((int)DxfCode.SoftPointerId,kv.Key));
                    int i = 1;
                    foreach (ObjectId id in kv.Value)
                    {
                        rb.Add(new TypedValue((int)DxfCode.SoftPointerId + i, id));
                        i++;
                    }

                    // Update or create an xrecord to store the data
                    Xrecord xrec;
                    bool newXrec = false;
                    if (dict.Contains(kXrecPrefix + xrecCount.ToString()))
                    {
                        // Open the existing object
                        DBObject obj =
                          tr.GetObject(
                            dict.GetAt(
                              kXrecPrefix + xrecCount.ToString()
                            ),
                            OpenMode.ForWrite
                          );
                        // Check whether it's an xrecord
                        xrec = obj as Xrecord;
                        if (xrec == null)
                        {
                            // Should never happen
                            // We only store xrecords in this dict
                            obj.Erase();
                            xrec = new Xrecord();
                            newXrec = true;
                        }
                    }
                    // No object existed - create a new one
                    else
                    {
                        xrec = new Xrecord();
                        newXrec = true;
                    }
                    xrec.XlateReferences = true;
                    xrec.Data = (ResultBuffer)rb;
                    if (newXrec)
                    {
                        dict.SetAt(
                          kXrecPrefix + xrecCount.ToString(),
                          xrec
                        );
                        tr.AddNewlyCreatedDBObject(xrec, true);
                    }
                    xrecCount++;
                }

                // Now erase the left-over xrecords
                bool finished = false;
                do
                {
                    if (dict.Contains(kXrecPrefix + xrecCount.ToString()))
                    {
                        DBObject obj = tr.GetObject(dict.GetAt(kXrecPrefix + xrecCount.ToString()), OpenMode.ForWrite);
                        obj.Erase();
                    }
                    else
                    {
                        finished = true;
                    }
                    xrecCount++;
                } while (!finished);
                tr.Commit();
            }
        }

        // Load the link information from a special
        // dictionary in the database
        public void LoadFromDatabase(Database db)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                // Try to find the link dictionary, but
                // do not create it if one isn't there
                ObjectId dictId = GetLinkDictionaryId(db, false);
                if (dictId.IsNull)
                {
                    ed.WriteMessage("\nCould not find link dictionary.");
                    return;
                }

                // By this stage we can assume the dictionary exists
                DBDictionary dict = (DBDictionary)tr.GetObject(dictId, OpenMode.ForRead);
                int xrecCount = 0;
                bool done = false;

                // Loop, reading the xrecords one-by-one
                while (!done)
                {
                    if (dict.Contains(kXrecPrefix + xrecCount.ToString()))
                    {
                        ObjectId recId = dict.GetAt(kXrecPrefix + xrecCount.ToString());
                        DBObject obj = tr.GetObject(recId, OpenMode.ForRead);
                        Xrecord xrec = obj as Xrecord;
                        if (xrec == null)
                        {
                            ed.WriteMessage("\nDictionary contains non-xrecords.");
                            return;
                        }
                        int i = 0;
                        ObjectId from = new ObjectId();
                        ObjectIdCollection to = new ObjectIdCollection();
                        foreach (TypedValue val in xrec.Data)
                        {
                            if (i == 0) from = (ObjectId)val.Value;
                            else
                            {
                                to.Add((ObjectId)val.Value);
                            }
                            i++;
                        }
                        // Validate the link info and add it to our
                        // internal data structure
                        AddValidatedLinks(db, from, to);
                        xrecCount++;
                    }
                    else
                    {
                        done = true;
                    }
                }
                tr.Commit();
            }
        }

        // Helper function to validate links before adding
        // them to the internal data structure
        private void AddValidatedLinks(Database db, ObjectId from, ObjectIdCollection to)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                try
                {
                    ObjectIdCollection newList = new ObjectIdCollection();

                    // Open the "from" object
                    DBObject obj = tr.GetObject(from, OpenMode.ForRead, false);
                    if (obj != null)
                    {
                        // Open each of the "to" objects
                        foreach (ObjectId id in to)
                        {
                            DBObject obj2;
                            try
                            {
                                obj2 = tr.GetObject(id, OpenMode.ForRead, false);
                                // Filter out the erased "to" objects
                                if (obj2 != null)
                                {
                                    newList.Add(id);
                                }
                            }
                            catch (System.Exception)
                            {
                                ed.WriteMessage("\nFiltered out link to an erased object.");
                            }
                        }
                        // Only if the "from" object and at least
                        // one "to" object exist (and are unerased)
                        // do we add an entry for them
                        if (newList.Count > 0)
                        {
                            m_dict.Add(from, newList);
                        }
                    }
                }
                catch (System.Exception)
                {
                    ed.WriteMessage("\nFiltered out link from an erased object.");
                }
                tr.Commit();
            }
        }

        // Helper function to get (optionally create)
        // the nested dictionary for our xrecord objects
        private ObjectId GetLinkDictionaryId(Database db, bool createIfNotExisting)
        {
            ObjectId appDictId = ObjectId.Null;

            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                // Our outer level ("company") dictionary
                // does not exist
                if (!nod.Contains(kCompanyDict))
                {
                    if (!createIfNotExisting)
                        return ObjectId.Null;

                    // Create both the "company" dictionary...
                    DBDictionary compDict = new DBDictionary();
                    nod.UpgradeOpen();
                    nod.SetAt(kCompanyDict, compDict);
                    tr.AddNewlyCreatedDBObject(compDict, true);

                    // ... and the inner "application" dictionary.
                    DBDictionary appDict = new DBDictionary();
                    appDictId = compDict.SetAt(kApplicationDict, appDict);
                    tr.AddNewlyCreatedDBObject(appDict, true);
                }
                else
                {
                    // Our "company" dictionary exists...
                    DBDictionary compDict = (DBDictionary)tr.GetObject(nod.GetAt(kCompanyDict), OpenMode.ForRead);
                    /// So check for our "application" dictionary
                    if (!compDict.Contains(kApplicationDict))
                    {
                        if (!createIfNotExisting)
                            return ObjectId.Null;

                        // Create the "application" dictionary
                        DBDictionary appDict = new DBDictionary();
                        compDict.UpgradeOpen();
                        appDictId = compDict.SetAt(kApplicationDict, appDict);
                        tr.AddNewlyCreatedDBObject(appDict, true);
                    }
                    else
                    {
                        // Both dictionaries already exist...
                        appDictId = compDict.GetAt(kApplicationDict);
                    }
                }
                tr.Commit();
            }
            return appDictId;
        }
    }
}