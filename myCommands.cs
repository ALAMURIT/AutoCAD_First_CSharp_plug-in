// (C) Copyright 2023 by  
// alamurit
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(AutoCAD_First_CSharp_plug_in.MyCommands))]

namespace AutoCAD_First_CSharp_plug_in
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        // Modal Command with localized name


        [CommandMethod("PlaceElbow")]
        public void PlaceElbow()
        {
            //Get document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            //Start transaction
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                //Prompt to select poly line
                PromptEntityOptions polyLineSelectionPrompt = new PromptEntityOptions("\nSelect center line representing GIB: ");
                polyLineSelectionPrompt.SetRejectMessage("\nInvalid selection!, pl select POLYLINE only.");
                polyLineSelectionPrompt.AddAllowedClass(typeof(Polyline), true);

                PromptEntityResult polyLineSelectionResult = doc.Editor.GetEntity(polyLineSelectionPrompt);

                if (polyLineSelectionResult.Status != PromptStatus.OK)
                {
                    return;
                }

                //open selected polyline for reading
                Polyline gibPolyine = tr.GetObject(polyLineSelectionResult.ObjectId, OpenMode.ForRead) as Polyline;

                //get points of that polyline
                Point3dCollection elbowLocationCollection = new Point3dCollection();    //Creates an array kind of object for collection of points
                for(int i = 0; i < gibPolyine.NumberOfVertices; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    Point3d tempElbowLocation = gibPolyine.GetPoint3dAt(i);
                    elbowLocationCollection.Add(tempElbowLocation);
                }

                //Prompt user to select block
                PromptEntityOptions blockSelectionPrompt = new PromptEntityOptions("\nSelect elbow block: ");
                blockSelectionPrompt.SetRejectMessage("\nInvalid selection. Pl select a BLOCK.");
                blockSelectionPrompt.AddAllowedClass(typeof(BlockReference), true);

                PromptEntityResult blockSelectionResult = doc.Editor.GetEntity(blockSelectionPrompt);

                if (blockSelectionResult.Status != PromptStatus.OK)
                {
                    return;
                }

                //open block reference for read
                BlockReference localElbowBlockReference = tr.GetObject(blockSelectionResult.ObjectId, OpenMode.ForRead) as BlockReference;

                //insert block ar each point
                //foreach (Point3d localElbowInsertionPoint in elbowLocationCollection)
                //{
                //    //set local block reference
                //    BlockReference localEblowReference = new BlockReference(localElbowInsertionPoint, localElbowBlockReference.BlockTableRecord);

                //    //set block reference properties
                //    localEblowReference.ScaleFactors = localElbowBlockReference.ScaleFactors;
                //    //localEblowReference.Rotation = localElbowBlockReference.Rotation;


                //    //Add new block reference to drawing
                //    BlockTableRecord elbowBlockTableRecord = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                //    elbowBlockTableRecord.AppendEntity(localEblowReference);
                //    tr.AddNewlyCreatedDBObject(localEblowReference, true);
                //}


                ////insert block at each point with orientation
                for (int j = 0; j < elbowLocationCollection.Count - 1; j++)
                {
                    Point3d currentVertex = elbowLocationCollection[j];
                    Point3d nextVertex = elbowLocationCollection[j + 1];

                    //set local block reference
                    BlockReference localEblowReference = new BlockReference(currentVertex, localElbowBlockReference.BlockTableRecord);

                    //set block reference properties
                    localEblowReference.ScaleFactors = localElbowBlockReference.ScaleFactors;
                    localEblowReference.Rotation = Vector3d.ZAxis.GetAngleTo(nextVertex - currentVertex, Vector3d.ZAxis);


                    //Add new block reference to drawing
                    BlockTableRecord elbowBlockTableRecord = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    elbowBlockTableRecord.AppendEntity(localEblowReference);
                    tr.AddNewlyCreatedDBObject(localEblowReference, true);
                }

                //commit transaction
                tr.Commit();
            }
        }

        [CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {
            // Put your command code here
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed;
            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("Hello, World! this is aCAD command~!");

            }
        }

    }

}
