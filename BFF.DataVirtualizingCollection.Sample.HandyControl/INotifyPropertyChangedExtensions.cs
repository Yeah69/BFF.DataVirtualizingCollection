﻿using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;

namespace BFF.Core.Extensions
{
    public static class NotifyPropertyChangedExtensions
    {
        public static IObservable<Unit> ObservePropertyChanges(
            this INotifyPropertyChanged source,
            string propertyName)
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    handler => handler.Invoke,
                    h => source.PropertyChanged += h,
                    h => source.PropertyChanged -= h)
                .Where(e => e.EventArgs.PropertyName == propertyName)
                .Select(_ => Unit.Default);
        }
    }
}
