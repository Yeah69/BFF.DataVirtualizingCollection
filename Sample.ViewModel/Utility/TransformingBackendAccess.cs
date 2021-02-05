using System;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.Utility
{
    internal class TransformingBackendAccess<TModel, TViewModel> : IBackendAccess<TViewModel>
    {
        internal static IBackendAccess<TViewModel> CreateTransformingBackendAccess(
            IBackendAccess<TModel> modelBackendAccess,
            Func<TModel[], TViewModel[]> transformingPageFactory,
            Func<TModel, TViewModel> transformingPlaceholderFactory)
        {
            return new TransformingBackendAccess<TModel, TViewModel>(
                modelBackendAccess,
                transformingPageFactory,
                transformingPlaceholderFactory);
        }
        
        private readonly IBackendAccess<TModel> _modelBackendAccess;
        private readonly Func<TModel[], TViewModel[]> _transformingPageFactory;
        private readonly Func<TModel, TViewModel> _transformingPlaceholderFactory;

        private TransformingBackendAccess(
            IBackendAccess<TModel> modelBackendAccess,
            Func<TModel[], TViewModel[]> transformingPageFactory,
            Func<TModel, TViewModel> transformingPlaceholderFactory)
        {
            _modelBackendAccess = modelBackendAccess;
            _transformingPageFactory = transformingPageFactory;
            _transformingPlaceholderFactory = transformingPlaceholderFactory;
        }

        public string Name => _modelBackendAccess.Name;
        public TViewModel[] PageFetch(int pageOffset, int pageSize)
        {
            return _transformingPageFactory(_modelBackendAccess.PageFetch(pageOffset, pageSize));
        }

        public TViewModel PlaceholderFetch(int pageOffset, int indexInsidePage)
        {
            return _transformingPlaceholderFactory(_modelBackendAccess.PlaceholderFetch(pageOffset, indexInsidePage));
        }

        public TViewModel PreloadingPlaceholderFetch(int pageOffset, int indexInsidePage)
        {
            return _transformingPlaceholderFactory(_modelBackendAccess.PreloadingPlaceholderFetch(pageOffset, indexInsidePage));
        }

        public int CountFetch()
        {
            return _modelBackendAccess.CountFetch();
        }
    }
}