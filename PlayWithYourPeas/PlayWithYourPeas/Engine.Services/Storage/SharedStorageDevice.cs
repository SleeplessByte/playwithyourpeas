using System;
#if !SILVERLIGHT
using Microsoft.Xna.Framework.Storage;
#endif

namespace PlayWithYourPeas.Engine.Services.Storage
{
	/// <summary>
	/// A SaveDevice used for non player-specific saving of data.
	/// </summary>
	public sealed class SharedStorageDevice : ExtendedStorageDevice
	{
		/// <summary>
		/// Derived classes should implement this method to call the Guide.BeginShowStorageDeviceSelector
		/// method with the desired parameters, using the given callback.
		/// </summary>
		/// <param name="callback">The callback to pass to Guide.BeginShowStorageDeviceSelector.</param>
		protected override void GetStorageDevice(AsyncCallback callback)
		{
#if SILVERLIGHT
            throw new NotSupportedException();
#else
			StorageDevice.BeginShowSelector(callback, null);
#endif
		}
	}
}