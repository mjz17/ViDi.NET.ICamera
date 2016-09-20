using System;
using System.Linq;
using System.Collections;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ViDi2;

namespace ViDi2.Camera
{

    /// <summary>
    /// ICameraParameter interface, standard interface to list and change camera parameters
    /// </summary>
    public interface ICameraParameter
    {
        /// <summary>
        /// User friendly name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Getter and setter to values, null setter if readonly
        /// </summary>
        object Value { get; set; }
        /// <summary>
        /// Indicates that the parameter is readonly, Value setter is null
        /// </summary>
        bool IsReadOnly { get; }
        /// <summary>
        /// Values that can be chosen in case of fixed parameters set.
        /// </summary>
        ReadOnlyCollection<object> Values { get; }
    }
    /// <summary>
    /// Interface specifying the capabilities of the camera
    /// </summary>
    public interface ICameraCapabilities
    {
        /// <summary>
        ///  Indicates that handle synchronous acquisition. (see ICamera Grab method)
        /// </summary>
        bool CanGrabSingle {get;}
        /// <summary>
        ///  Indicates that handle live asynchronous acquisition. Images are transmitted the ImageGrabbed Event. (see ICamera StartLive / StopLive methods)
        /// </summary>
        bool CanGrabContinuous { get; }
        /// <summary>
        /// Indicates that the camera can save and load the current parameter set to a file
        /// </summary>
        bool CanSaveParametersToFile { get; }
        /// <summary>
        /// Indicates that the camera can save the current parameter set to memory
        /// </summary>
        bool CanSaveParametersToDevice { get; }
    }

    /// <summary>
    /// Standard implementation of ICameraParameter interface
    /// </summary>
    public class CameraParameter : ICameraParameter
    {
        public CameraParameter(string name, Func<object> get, Action<object> set, 
                               IEnumerable<object> values = null)
        {
            Name = name;
            this.setter = set;
            this.getter = get;
            Values = new ReadOnlyCollection<object>(values != null ? 
                values.ToList() : new List<object>());
        }

        public string Name { get; private set; }

        public object Value
        {
            get
            {
                return getter();
            }
            set
            {
                if (setter == null)
                    throw new NotSupportedException("This is a read-only parameter");

                setter(value);
            }
        }

        public bool IsReadOnly { get { return setter == null; } }

        public ReadOnlyCollection<object> Values { get; private set; }

        Action<object> setter;
        Func<object> getter;
    }

    public interface ICamera
    {
        /// <summary>
        /// Returns the name of the camera
        /// </summary>
        string Name { get; }  

        /// <summary>
        /// Opens the camera
        /// </summary>
        void Open();

        /// <summary>
        ///  Returns true if the camera is opened
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Closes the camera
        /// </summary>
        void Close();
        
        /// <summary>
        /// Returns true if the camera is live 
        /// </summary>
        bool IsGrabbingContinuous { get; }   

        /// <summary>
        /// Stats live asynchronous acquisition. Images are transmitted the ImageGrabbed Event
        /// </summary>
        void StartGrabContinuous();

        /// <summary>
        /// Stops the asynchronous acquisition.
        /// </summary>
        void StopGrabContinuous();

        /// <summary>
        /// Loads the parameters from a filename
        /// </summary>
        /// <param name="parameters_file"></param>
        void LoadParameters(string parametersFile);

        /// <summary>
        /// Saves the camera parameters to the given filename
        /// </summary>
        /// <param name="parameters_file"></param>
        void SaveParameters(string parametersFile);

        /// <summary>
        /// Saves the parameters to the camera memory
        /// </summary>
        void SaveParametersToDevice();

        /// <summary>
        /// Synchronously grab an image
        /// </summary>
        /// <returns></returns>
        IImage GrabSingle();

        /// <summary>
        /// Event occuring when a new frame is acquired
        /// </summary>
        event ImageGrabbedHandler ImageGrabbed;
        
        /// <summary>
        /// Returns the capabilities of the camera
        /// </summary>
        ICameraCapabilities Capabilities { get; }

        /// <summary>
        /// Returns a Ienumerable containing all  of the camera
        /// </summary>
        IEnumerable<ICameraParameter> Parameters { get; }

        /// <summary>
        /// Reference to Parent Camera Provider
        /// </summary>
        ICameraProvider Provider { get; }
    }

    /// <summary>
    /// Handler called when an image has been grabbed
    /// </summary>
    /// <param name="image">the grabbed image</param>
    public delegate void ImageGrabbedHandler(ICamera sender, ViDi2.IImage image);
}
