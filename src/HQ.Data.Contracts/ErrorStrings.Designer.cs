﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HQ.Data.Contracts {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ErrorStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("HQ.Data.Contracts.ErrorStrings", typeof(ErrorStrings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple errors encountered..
        /// </summary>
        public static string AggregateErrors {
            get {
                return ResourceManager.GetString("AggregateErrors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is not a valid attributes of &apos;{1}&apos;..
        /// </summary>
        public static string FieldToPropertyMismatch {
            get {
                return ResourceManager.GetString("FieldToPropertyMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Page range starts at 1..
        /// </summary>
        public static string PageRangeInvalid {
            get {
                return ResourceManager.GetString("PageRangeInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Per page request is higher than the maximum allowed..
        /// </summary>
        public static string PerPageTooHigh {
            get {
                return ResourceManager.GetString("PerPageTooHigh", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Batch size cannot be larger than {0} items..
        /// </summary>
        public static string PostBatchSizeExceeded {
            get {
                return ResourceManager.GetString("PostBatchSizeExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must provide a resource to save it..
        /// </summary>
        public static string ResourceMissingInSave {
            get {
                return ResourceManager.GetString("ResourceMissingInSave", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided implementationType is not an open generic implementationType.
        /// </summary>
        public static string TypeIsNotAnOpenGeneric {
            get {
                return ResourceManager.GetString("TypeIsNotAnOpenGeneric", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation failed..
        /// </summary>
        public static string ValidationFailed {
            get {
                return ResourceManager.GetString("ValidationFailed", resourceCulture);
            }
        }
    }
}
