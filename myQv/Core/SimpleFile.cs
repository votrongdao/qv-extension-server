﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myQv.Core
{
    public class SimpleFile
    {
        public static int Copy(string sourcePath, string search, string targetPath)
        {
            int r = 0;

            if (!System.IO.Directory.Exists(targetPath))
                System.IO.Directory.CreateDirectory(targetPath);

            string[] directories = System.IO.Directory.GetDirectories(sourcePath, search);
            foreach (string s in directories)
            {
                r += SimpleFile.Copy(s, search, System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(s)));
            }

            string[] files = System.IO.Directory.GetFiles(sourcePath, search);
            foreach (string s in files)
            {
                System.IO.File.Copy(s, System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(s)), true);
                r += 1;
            }

            return r;
        }

        public static int Move(string sourcePath, string search, string targetPath)
        {
            int r = 0;

            if (!System.IO.Directory.Exists(targetPath))
                System.IO.Directory.CreateDirectory(targetPath);

            string[] directories = System.IO.Directory.GetDirectories(sourcePath, search);
            foreach (string s in directories)
            {
                r += SimpleFile.Move(s, search, System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(s)));
            }

            string[] files = System.IO.Directory.GetFiles(sourcePath, search);
            foreach (string s in files)
            {
                string destFile = System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(s));

                if (System.IO.File.Exists(destFile))
                    System.IO.File.Delete(destFile);

                System.IO.File.Move(s, destFile);

                r += 1;
            }

            return r;

        }

    }
}