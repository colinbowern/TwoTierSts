using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace SecurityTokenService.ModelBinders
{
    /// <summary>
    /// Provides a convention based model binder provider.
    /// </summary>
    public class ConventionModelBinderProvider : IModelBinderProvider
    {
        private static readonly Dictionary<Type, Type> binders = new Dictionary<Type, Type>();
        private static readonly object syncLock = new object();

        /// <summary>
        /// Manually register ModelBinders
        /// </summary>
        /// <param name="modelType">The type of object we're binding to</param>
        /// <param name="binderType">The type of the IModelBinder that can bind the model</param>
        public static void RegisterType(Type modelType, Type binderType)
        {
            lock (syncLock)
            {
                binders[modelType] = binderType;
            }
        }

        /// <summary>
        /// Interface for IModelBinderProvider.GetBinder.
        /// Checks Binders for the passed type else loop through the assembly and look for a binder
        /// </summary>
        /// <param name="modelType">The type of object we're going to bind to</param>
        /// <returns>An instance of IModelBinder if it can be found.</returns>
        public IModelBinder GetBinder(Type modelType)
        {
            Type binderType = null;
            lock (syncLock)
            {
                if (binders.ContainsKey(modelType))
                {
                    binderType = binders[modelType];
                }
                else
                {
                    binderType = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                  where type.Name == modelType.Name + "ModelBinder"
                                  select type).FirstOrDefault();
                    if (binderType != null)
                    {
                        binders[modelType] = binderType;
                    }
                }
            }

            if (binderType == null)
                return null;

            var binder = (IModelBinder)DependencyResolver.Current.GetService(binderType);
            return binder;
        }
    }
}
