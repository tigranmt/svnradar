using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace SubversionStatistics
{
    /// <summary>
    /// Manages adorner Binding and Unbinding to specified element
    /// </summary>
    public static class AdornerManager
    {
        static AdornerLayer _adornerLayer = null;
        static UIElement _element = null;
        static Adorner _adorner = null;

        /// <summary>
        /// Adorner Layer, if ther is not any, Null reference exception will be thrown
        /// </summary>
        static AdornerLayer Layer
        {
            get
            {
                if (_adornerLayer == null)
                {
                    _adornerLayer = AdornerLayer.GetAdornerLayer(_element);
                    if (_adornerLayer == null)
                        throw new NullReferenceException("Can not find adorner layer from the specified UI element");
                }

                return _adornerLayer;
            }
        }

        /// <summary>
        /// Binds specified adorner to the specified UI element
        /// </summary>
        /// <param name="uiElement">UI element to which the adorenr should be binded</param>
        /// <param name="adorner">Adorner object</param>
        public static void AddAdorner(UIElement uiElement, Adorner adorner)
        {
            _element = uiElement;
            _adorner = adorner;
            Layer.Add(adorner);
        }

        /// <summary>
        /// removes previously added adorner from the parent layer
        /// </summary>
        public static void RemoveAdorner()
        {
            if (_adornerLayer == null)
                throw new NullReferenceException("Layer object is Null");
            if (_adorner == null)
                throw new NullReferenceException("Adorner object's reference is Null");

            Layer.Remove(_adorner);
        }

    }
}
