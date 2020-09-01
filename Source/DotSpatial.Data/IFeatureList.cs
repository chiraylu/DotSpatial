// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DotSpatial.Data
{
    /// <summary>
    /// IFeatureList
    /// </summary>
    public interface IFeatureList : IList<IFeature>, INotifyCollectionChanged
    {
        #region Events

        /// <summary>
        /// Occurs before a feature is removed from the list.
        /// </summary>
        event EventHandler<PreviewRemoveFeatureEventArgs> PreviewRemoveFeature;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether or not the events have been suspended.
        /// </summary>
        bool EventsSuspended { get; }

        /// <summary>
        /// Gets or sets a value indicating whether attribute Table information will be copied when features will be added to the list.
        /// This will allow the attributes to be loaded in a more on-demand later.
        /// </summary>
        bool IncludeAttributes { get; set; }

        /// <summary>
        /// Gets the parent featureset for this list.
        /// </summary>
        IFeatureSet Parent { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Resumes events
        /// </summary>
        void ResumeEvents();

        /// <summary>
        /// Temporarilly disables events
        /// </summary>
        void SuspendEvents();

        /// <summary>
        /// This is a re-expression of the features using a strong typed
        /// list. This may be the inner list or a copy depending on
        /// implementation.
        /// </summary>
        /// <returns>The features as a List of IFeature.</returns>
        List<IFeature> ToList();

        void Add(IEnumerable<IFeature> features);
        void Insert(Dictionary<int, IFeature> fidFeaturePairs);
        void Remove(IEnumerable<IFeature> features);
        #endregion
    }
}