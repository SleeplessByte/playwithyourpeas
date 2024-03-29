﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PlayWithYourPeas.Engine.Services.Storage {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PlayWithYourPeas.Engine.Services.Storage.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No storage device was selected. A storage device is required to continue..
        /// </summary>
        internal static string forceCanceledReselectionMessage {
            get {
                return ResourceManager.GetString("forceCanceledReselectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The storage device was disconnected. A storage device is required to continue..
        /// </summary>
        internal static string forceDisconnectedReselectionMessage {
            get {
                return ResourceManager.GetString("forceDisconnectedReselectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SaveDevice requries gamer services to operate. Add the GamerServicesComponent to your game..
        /// </summary>
        internal static string NeedGamerService {
            get {
                return ResourceManager.GetString("NeedGamerService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No. Continue without device..
        /// </summary>
        internal static string No_Continue_without_device {
            get {
                return ResourceManager.GetString("No_Continue_without_device", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ok.
        /// </summary>
        internal static string Ok {
            get {
                return ResourceManager.GetString("Ok", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No storage device was selected. You can continue without a device, but you will not be able to save. Would you like to select a storage device?.
        /// </summary>
        internal static string promptForCancelledMessage {
            get {
                return ResourceManager.GetString("promptForCancelledMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The storage device was disconnected. You can continue without a device, but you will not be able to save. Would you like to select a storage device?.
        /// </summary>
        internal static string promptForDisconnectedMessage {
            get {
                return ResourceManager.GetString("promptForDisconnectedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reselect Storage Device?.
        /// </summary>
        internal static string Reselect_Storage_Device {
            get {
                return ResourceManager.GetString("Reselect_Storage_Device", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Storage Device Required.
        /// </summary>
        internal static string Storage_Device_Required {
            get {
                return ResourceManager.GetString("Storage_Device_Required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to StorageDevice is not valid..
        /// </summary>
        internal static string StorageDevice_is_not_valid {
            get {
                return ResourceManager.GetString("StorageDevice_is_not_valid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Yes. Select new device..
        /// </summary>
        internal static string Yes_Select_new_device {
            get {
                return ResourceManager.GetString("Yes_Select_new_device", resourceCulture);
            }
        }
    }
}
