using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Blurate
{
    [Activity(Label = "OpenFileAuto", Name = "advkern.blurate")]
    public class OpenFileAuto : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            var intent = new Intent(this, typeof(MainActivity))
                   .SetFlags(ActivityFlags.ReorderToFront);

            //TODO: This does work yet for network drives.

            if ((Intent.Action == Intent.ActionView) || (Intent.Action == Intent.ActionPick))
            {
                if (System.IO.File.Exists(Intent.Data.Path))
                {
                    var fileUrl = GetExternalFilesDir(Intent.Data.Path);
                    string currentInputFile = Intent.Data.Path;

                    string importedFltr = BlrtkrnlRdWr.ImportBlrtkrnl(currentInputFile);
                    if (importedFltr == null) return;
                    //bool changeInProgress = true;
                    //string newFilter = importedFltr.Split('=')[0].Trim();
                    DataHolder.setData(importedFltr);
                }
                else
                {
                    Intent i = Intent;
                    if (i == null) return;
                    Android.Net.Uri u = i.Data;
                    if (u == null) return;
                    String scheme = u.Scheme;

                    if (ContentResolver.SchemeContent.Equals(scheme))
                    {
                        try
                        {
                            ContentResolver cr = ContentResolver;
                            Android.Content.Res.AssetFileDescriptor afd = cr.OpenAssetFileDescriptor(u, "r");
                            long length = afd.Length;
                            byte[] filedata = new byte[(int)length];
                            System.IO.Stream is_ = cr.OpenInputStream(u);
                            if (is_ == null) return;
                            try
                            {
                                is_.Read(filedata, 0, (int)length);
                                var str = BlrtkrnlRdWr.ImportBlrtkrnl_fromByteArray(filedata);
                                //var str = System.Text.Encoding.Default.GetString(filedata);
                                if (str == null)
                                {
                                    Android.Graphics.Bitmap tempCMp = Android.Graphics.BitmapFactory.DecodeByteArray(filedata, 0, (int)length);
                                    DataHolder.setImage(tempCMp);
                                }
                                DataHolder.setData(str);
                            }
                            catch (Exception e)
                            {
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            return;
                        }
                    }




                    /*

                    Android.Net.Uri returnUri = Intent.Data;
                    String mimeType = ContentResolver.GetType(returnUri);
                    Android.Database.ICursor returnCursor =
                            ContentResolver.Query(returnUri, null, null, null, null);
                    int nameIndex = returnCursor.GetColumnIndex(Android.Provider.OpenableColumns.DisplayName);
                    int sizeIndex = returnCursor.GetColumnIndex(Android.Provider.OpenableColumns.Size);
                    returnCursor.MoveToFirst ();
                    //TextView nameView = (TextView)FindViewById(R.id.filename_text);
                    //TextView sizeView = (TextView)FindViewById(R.id.filesize_text);
                    //nameView.setText(returnCursor.getString(nameIndex));
                    //sizeView.setText(Long.toString(returnCursor.getLong(sizeIndex)));

                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        string newPath = System.IO.File.ReadAllText(returnUri.Path);
                        
                        string s = client.DownloadString(newPath);
                        DataHolder.setData(s);
                    }

                    //string filter = readTextFromUri(returnUri);
                    //DataHolder.setData(filter);

                */

                }

                
            }

            Finish();
            StartActivity(intent);
            //StartActivity(Intent);

            // Create your application here
        }

        private String readTextFromUri(Android.Net.Uri uri) 
        {
            System.IO.Stream inputStream = ContentResolver.OpenInputStream(uri);
            Java.IO.BufferedReader reader = new Java.IO.BufferedReader(new Java.IO.InputStreamReader(inputStream));
            StringBuilder stringBuilder = new StringBuilder();
            String line;
            while ((line = reader.ReadLine()) != null)
            {
                stringBuilder.Append(line);
            }
            return stringBuilder.ToString();
        }

    }


}