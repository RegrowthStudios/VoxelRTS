using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RTSEngine.Data {
    public static class DataLibrary {
        private static readonly string[] rootDirectories = {
            "resources"
        };

        public static ResourceCollection Resources { get; private set; }

        public static bool IsInitialized { get; private set; }

        private static void Initialize(string root) {
            // Check For The Root Directory
            DirectoryInfo dirRoot = new DirectoryInfo(root);
            if(!dirRoot.Exists)
                throw new DirectoryNotFoundException("Root Directory Does Not Exist");


        }
        public static bool TryInitialize(string root, out string error) {
            error = null;
            try {
                Initialize(root);
                IsInitialized = true;
            }
            catch(Exception e) {
                error = e.Message;
                IsInitialized = false;
            }
            return IsInitialized;
        }
    }
}