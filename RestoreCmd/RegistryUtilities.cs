using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestoreCmd
{
    public static class RegistryUtilities
    {
        /// <summary>
        /// Renames a subkey of the passed in registry key since 
        /// the Framework totally forgot to include such a handy feature.
        /// </summary>
        /// <param name="regKey">The RegistryKey that contains the subkey 
        /// you want to rename (must be writeable)</param>
        /// <param name="subKeyName">The name of the subkey that you want to rename
        /// </param>
        /// <param name="newSubKeyName">The new name of the RegistryKey</param>
        /// <returns>True if succeeds</returns>
        public static bool RenameSubKey(RegistryKey parentKey,
            string subKeyName, string newSubKeyName)
        {
            if (CopyKey(parentKey, subKeyName, newSubKeyName))
            {
                parentKey.DeleteSubKeyTree(subKeyName);
            }
            
            return true;
        }

        /// <summary>
        /// Copy a registry key.  The parentKey must be writeable.
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="keyNameToCopy"></param>
        /// <param name="newKeyName"></param>
        /// <returns></returns>
        public static bool CopyKey(RegistryKey parentKey,
            string keyNameToCopy, string newKeyName)
        {
            using (var destinationKey = parentKey.CreateSubKey(newKeyName))
            using (var sourceKey = parentKey.OpenSubKey(keyNameToCopy))
            {
                if (sourceKey == null) return false;
                RecurseCopyKey(sourceKey, destinationKey);
            }
            return true;
        }

        private static void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            //copy all the values
            foreach (string valueName in sourceKey.GetValueNames())
            {
                object objValue = sourceKey.GetValue(valueName);
                RegistryValueKind valKind = sourceKey.GetValueKind(valueName);
                destinationKey.SetValue(valueName, objValue, valKind);
            }

            //For Each subKey 
            //Create a new subKey in destinationKey 
            //Call myself 
            foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                using (var sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName))
                using (var destSubKey = destinationKey.CreateSubKey(sourceSubKeyName))
                {
                    RecurseCopyKey(sourceSubKey, destSubKey);
                }
            }
        }
    }
}
