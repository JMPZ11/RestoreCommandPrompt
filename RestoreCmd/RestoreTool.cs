using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RestoreCmd
{
    static class RestoreTool
    {

        static void AddWriteAccess(string keyName)
        {
            try
            {
                TokenManipulator.AddPrivilege("SeRestorePrivilege");
                TokenManipulator.AddPrivilege("SeBackupPrivilege");
                TokenManipulator.AddPrivilege("SeTakeOwnershipPrivilege");

                //HKEY_CLASSES_ROOT
                //HKEY_CURRENT_USER
                //HKEY_LOCAL_MACHINE

                RegistryKey root = null;
                string localName = null;

                if (keyName.StartsWith("HKEY_CLASSES_ROOT"))
                {
                    root = Registry.ClassesRoot;
                    localName = keyName.Substring(18);
                }
                else if (keyName.StartsWith("HKEY_CURRENT_USER"))
                {
                    root = Registry.CurrentUser;
                    localName = keyName.Substring(18);
                }
                else if (keyName.StartsWith("HKEY_LOCAL_MACHINE"))
                {
                    root = Registry.LocalMachine;
                    localName = keyName.Substring(19);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported registry root");
                }

                using (var thing = root.OpenSubKey(localName, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.TakeOwnership))
                {
                    if (thing != null)
                    {
                        //change owner to Administrators and give administrators full rights
                        var acl = thing.GetAccessControl();
                        var admins = new System.Security.Principal.NTAccount("Administrators");
                        acl.SetOwner(admins);
                        thing.SetAccessControl(acl);

                        var acl2 = thing.GetAccessControl();
                        acl2.AddAccessRule(new RegistryAccessRule(admins, RegistryRights.FullControl, AccessControlType.Allow));
                        thing.SetAccessControl(acl2);

                        Console.WriteLine("Added write permissions for Administrator");
                    }

                }

                using (var thing = root.OpenSubKey(localName, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.TakeOwnership | RegistryRights.FullControl))
                {
                    if (thing != null)
                    {
                        //change ownership back to TrustedInstaller
                        var acl = thing.GetAccessControl();
                        var account = new System.Security.Principal.NTAccount(@"NT SERVICE\TrustedInstaller");
                        acl.SetOwner(account);
                        thing.SetAccessControl(acl);
                        Console.WriteLine("Restored Ownership to TrustedInstaller");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                TokenManipulator.RemovePrivilege("SeRestorePrivilege");
                TokenManipulator.RemovePrivilege("SeBackupPrivilege");
                TokenManipulator.RemovePrivilege("SeTakeOwnershipPrivilege");
            }
        }



        public static bool CheckFolderStatus()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Directory\shell\cmd"))
            {
                var val = key.GetValue("ShowBasedOnVelocityId");
                if (val == null)
                {
                    return false;
                }
                return true;
            }
        }

        public static bool CheckFolderBackgroundStatus()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Directory\Background\shell\cmd"))
            {
                var val = key.GetValue("ShowBasedOnVelocityId");
                if (val == null)
                {
                    return false;
                }
                return true;
            }
        }

        public static bool CheckExplorerRibbonStatus()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.Powershell"))
            {
                try
                {
                    var val = key.GetValue("MUIVerb") as string;
                    if (val == null || !val.Contains("37415"))
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }


        public static void EnableFolder()
        {
            try
            {
                EnableFolderInternal();
            }
            catch (Exception)
            {
                AddWriteAccess(@"HKEY_CLASSES_ROOT\Directory\shell\cmd");
                EnableFolderInternal();
            }
        }

        public static void DisableFolder()
        {
            try
            {
                DisableFolderInternal();
            }
            catch (Exception)
            {
                AddWriteAccess(@"HKEY_CLASSES_ROOT\Directory\shell\cmd");
                DisableFolderInternal();
            }
        }

        public static void EnableFolderBackground()
        {
            try
            {
                EnableFolderBackgroundInternal();
            }
            catch (Exception)
            {
                AddWriteAccess(@"HKEY_CLASSES_ROOT\Directory\Background\shell\cmd");
                EnableFolderBackgroundInternal();
            }
        }

        public static void DisableFolderBackground()
        {
            try
            {
                DisableFolderBackgroundInternal();
            }
            catch (Exception)
            {
                AddWriteAccess(@"HKEY_CLASSES_ROOT\Directory\Background\shell\cmd");
                DisableFolderBackgroundInternal();
            }
        }


        public static void ReplacePowershellWithCmd()
        {
            AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell");
            AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.Powershell");
            AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.PowershellAsAdmin");
            ReplacePowershellRibbonInternal();


            //try
            //{

            //}
            //catch (Exception)
            //{
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell");
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.Powershell");
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.PowershellAsAdmin");
            //    ReplacePowershellRibbonInternal();
            //}
        }

        public static void RestorePowershellRibbon()
        {
            AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell");
            AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.Powershell");
            AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.PowershellAsAdmin");
            RestorePowershellRibbonInternal();

            //try
            //{
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell");
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.Powershell");
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.PowershellAsAdmin");
            //    RestorePowershellRibbonInternal();
            //}
            //catch (Exception)
            //{
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell");
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.Powershell");
            //    AddWriteAccess(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\Windows.MultiVerb.PowershellAsAdmin");
            //    RestorePowershellRibbonInternal();
            //}
        }




        static void EnableFolderInternal()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Directory\shell\cmd", true))
            {
                var val = key.GetValue("HideBasedOnVelocityId");
                if (val != null)
                {
                    key.SetValue("ShowBasedOnVelocityId", val, RegistryValueKind.DWord);
                    key.DeleteValue("HideBasedOnVelocityId");
                }
            }
        }

        static void DisableFolderInternal()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Directory\shell\cmd", true))
            {
                var val = key.GetValue("ShowBasedOnVelocityId");
                if (val != null)
                {
                    key.SetValue("HideBasedOnVelocityId", val, RegistryValueKind.DWord);
                    key.DeleteValue("ShowBasedOnVelocityId");
                }
            }
        }

        static void EnableFolderBackgroundInternal()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Directory\Background\shell\cmd", true))
            {
                var val = key.GetValue("HideBasedOnVelocityId");
                if (val != null)
                {
                    key.SetValue("ShowBasedOnVelocityId", val, RegistryValueKind.DWord);
                    key.DeleteValue("HideBasedOnVelocityId");
                }
            }
        }

        static void DisableFolderBackgroundInternal()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Directory\Background\shell\cmd", true))
            {
                var val = key.GetValue("ShowBasedOnVelocityId");
                if (val != null)
                {
                    key.SetValue("HideBasedOnVelocityId", val, RegistryValueKind.DWord);
                    key.DeleteValue("ShowBasedOnVelocityId");
                }
            }
        }


        static void ReplacePowershellRibbonInternal()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell", true))
            {
                //backup existing powershell config
                RegistryUtilities.RenameSubKey(key, "Windows.MultiVerb.Powershell", "Windows.MultiVerb.Powershell-backup");
                RegistryUtilities.RenameSubKey(key, "Windows.MultiVerb.PowershellAsAdmin", "Windows.MultiVerb.PowershellAsAdmin-backup");

                //copy the cmd config over to powershell

                RegistryUtilities.CopyKey(key, "Windows.MultiVerb.cmd", "Windows.MultiVerb.Powershell");
                RegistryUtilities.CopyKey(key, "Windows.MultiVerb.cmdPromptAsAdministrator", "Windows.MultiVerb.PowershellAsAdmin");
            }
        }


        static void RestorePowershellRibbonInternal()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell", true))
            {
                //check if the backup exists before doing anything
                var kns = key.GetSubKeyNames();

                if (!kns.Contains("Windows.MultiVerb.Powershell-backup") || !kns.Contains("Windows.MultiVerb.PowershellAsAdmin-backup"))
                {
                    throw new InvalidOperationException("Cannot restore powershell when backup is missing");
                }

                key.DeleteSubKey("Windows.MultiVerb.Powershell");
                key.DeleteSubKey("Windows.MultiVerb.PowershellAsAdmin");

                RegistryUtilities.CopyKey(key, "Windows.MultiVerb.Powershell-backup", "Windows.MultiVerb.Powershell");
                RegistryUtilities.CopyKey(key, "Windows.MultiVerb.PowershellAsAdmin-backup", "Windows.MultiVerb.PowershellAsAdmin");
            }
        }




    }
}
