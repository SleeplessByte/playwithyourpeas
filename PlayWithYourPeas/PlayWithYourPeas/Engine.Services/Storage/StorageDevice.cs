﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#if !SILVERLIGHT
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;
#endif

namespace PlayWithYourPeas.Engine.Services.Storage
{
	/// <summary>
	/// A base class for an object that maintains a StorageDevice.
	/// </summary>
	/// <remarks>
	/// We implement IGameComponent and IUpdateable rather than deriving from GameComponent
	/// just to simplify our constructor and remove the need to pass the Game to it.
	/// 
	/// The implementation for the synchronous actions of ISaveDevice are found in
	/// SaveDeviceSynchronous.cs.
	/// 
	/// The implementation for the asynchronous actions of IAsyncSaveDevice are found in
	/// SaveDeviceAsync.cs.
	/// </remarks>
	public abstract partial class ExtendedStorageDevice : IGameComponent, IUpdateable, IAsyncStorageDevice
	{
		#region Static Strings

		// Strings for various message boxes
		private static String promptForCancelledMessage;
		private static String forceCancelledReselectionMessage;
		private static String promptForDisconnectedMessage;
		private static String forceDisconnectedReselectionMessage;
		private static String deviceRequiredTitle;
		private static String deviceOptionalTitle;
		private static readonly String[] deviceOptionalOptions = new String[2];
		private static readonly String[] deviceRequiredOptions = new String[1];

		/// <summary>
		/// Gets or sets the message displayed when the user is asked if they want
		/// to select a storage device after cancelling the storage device selector.
		/// </summary>
		public static String PromptForCancelledMessage
		{
			get { return promptForCancelledMessage; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					promptForCancelledMessage = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the message displayed when the user is told they must
		/// select a storage device after cancelling the storage device selector.
		/// </summary>
		public static String ForceCancelledReselectionMessage
		{
			get { return forceCancelledReselectionMessage; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					forceCancelledReselectionMessage = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the message displayed when the user is asked if they
		/// want to select a new storage device after the storage device becomes
		/// disconnected.
		/// </summary>
		public static String PromptForDisconnectedMessage
		{
			get { return promptForDisconnectedMessage; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					promptForDisconnectedMessage = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the message displayed when the user is told they must
		/// select a new storage device after the storage device becomes disconnected.
		/// </summary>
		public static String ForceDisconnectedReselectionMessage
		{
			get { return forceDisconnectedReselectionMessage; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					forceDisconnectedReselectionMessage = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the title displayed when the user is required to choose
		/// a storage device.
		/// </summary>
		public static String DeviceRequiredTitle
		{
			get { return deviceRequiredTitle; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					deviceRequiredTitle = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the title displayed when the user is asked if they want
		/// to choose a storage device.
		/// </summary>
		public static String DeviceOptionalTitle
		{
			get { return deviceOptionalTitle; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					deviceOptionalTitle = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the text used for the "Ok" button when the user is told
		/// they must select a storage device.
		/// </summary>
		public static String OkOption
		{
			get { return deviceRequiredOptions[0]; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					deviceRequiredOptions[0] = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the text used for the "Yes" button when the user is asked
		/// if they want to select a storage device.
		/// </summary>
		public static String YesOption
		{
			get { return deviceOptionalOptions[0]; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					deviceOptionalOptions[0] = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		/// <summary>
		/// Gets or sets the text used for the "No" button when the user is asked
		/// if they want to select a storage device.
		/// </summary>
		public static String NoOption
		{
			get { return deviceOptionalOptions[1]; }
			set
			{
				if (!String.IsNullOrEmpty(value))
					deviceOptionalOptions[1] = value.Length < 256 ? value : value.Substring(0, 256);
			}
		}

		#endregion

		// the update order for the component and its enabled state
		private int updateOrder;
		private Boolean enabled = true;

		// was the device connected last frame?
		private Boolean deviceWasConnected;

		// the current state of the SaveDevice
		private StorageDevicePromptState state = StorageDevicePromptState.None;

		// we store the callbacks as fields to reduce run-time allocation and garbage
		private readonly AsyncCallback storageDeviceSelectorCallback;
		private readonly AsyncCallback forcePromptCallback;
		private readonly AsyncCallback reselectPromptCallback;

		// arguments for our two types of events
		private readonly StorageDevicePromptEventArgs promptEventArgs = new StorageDevicePromptEventArgs();
		private readonly StorageDeviceEventArgs eventArgs = new StorageDeviceEventArgs();

		// the actual storage device
#if !SILVERLIGHT
		private StorageDevice storageDevice;
#endif
		/// <summary>
		/// Gets whether or not the device is in a state to receive file operation method calls.
		/// </summary>
		public Boolean IsReady
		{
#if SILVERLIGHT
            get { return true; }
#else
			get { return storageDevice != null && storageDevice.IsConnected; }
#endif
		}

		/// <summary>
		/// Gets or sets a value indicating whether the SaveDevice is enabled for use.
		/// </summary>
		public Boolean Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled != value)
				{
					enabled = value;
					if (EnabledChanged != null)
						EnabledChanged(this, null);
				}
			}
		}

		/// <summary>
		/// Gets or sets the order in which the SaveDevice is updated
		/// in the game. Components with a lower UpdateOrder are updated
		/// first.
		/// </summary>
		public int UpdateOrder
		{
			get { return updateOrder; }
			set
			{
				if (updateOrder != value)
				{
					updateOrder = value;
					if (UpdateOrderChanged != null)
						UpdateOrderChanged(this, null);
				}
			}
		}

		/// <summary>
		/// Invoked when a StorageDevice is selected.
		/// </summary>
		public event EventHandler<EventArgs> DeviceSelected;

		/// <summary>
		/// Invoked when a StorageDevice selector is canceled.
		/// </summary>
		public event EventHandler<StorageDeviceEventArgs> DeviceSelectorCanceled;

		/// <summary>
		/// Invoked when the user closes a prompt to reselect a StorageDevice.
		/// </summary>
		public event EventHandler<StorageDevicePromptEventArgs> DeviceReselectPromptClosed;

		/// <summary>
		/// Invoked when the StorageDevice is disconnected.
		/// </summary>
		public event EventHandler<StorageDeviceEventArgs> DeviceDisconnected;

		/// <summary>
		/// Fired when the Enabled property has been changed.
		/// </summary>
		public event EventHandler<EventArgs> EnabledChanged;

		/// <summary>
		/// Fired when the UpdateOrder property has been changed.
		/// </summary>
		public event EventHandler<EventArgs> UpdateOrderChanged;

		static ExtendedStorageDevice()
		{
			// reset the Strings to fill in the defaults
            ExtendedStorageDevice.OkOption = Strings.Ok;
            ExtendedStorageDevice.YesOption = Strings.Yes_Select_new_device;
            ExtendedStorageDevice.NoOption = Strings.No_Continue_without_device;
            ExtendedStorageDevice.DeviceOptionalTitle = Strings.Reselect_Storage_Device;
            ExtendedStorageDevice.DeviceRequiredTitle = Strings.Storage_Device_Required;
            ExtendedStorageDevice.ForceDisconnectedReselectionMessage = Strings.forceDisconnectedReselectionMessage;
            ExtendedStorageDevice.PromptForDisconnectedMessage = Strings.promptForDisconnectedMessage;
            ExtendedStorageDevice.ForceCancelledReselectionMessage = Strings.forceCanceledReselectionMessage;
            ExtendedStorageDevice.PromptForCancelledMessage = Strings.promptForCancelledMessage;
		}

		/// <summary>
		/// Creates a new SaveDevice.
		/// </summary>
		protected ExtendedStorageDevice()
		{
			storageDeviceSelectorCallback = StorageDeviceSelectorCallback;
			reselectPromptCallback = ReselectPromptCallback;
			forcePromptCallback = ForcePromptCallback;
		}

		/// <summary>
		/// Allows the SaveDevice to initialize itself.
		/// </summary>
		public virtual void Initialize() { }

		/// <summary>
		/// Flags the SaveDevice to prompt for a storage device on the next Update.
		/// </summary>
		public void PromptForDevice()
		{
			// we only let the programmer show the selector if the 
			// SaveDevice isn't busy doing something else.
			if (state == StorageDevicePromptState.None)
				state = StorageDevicePromptState.ShowSelector;
		}

		/// <summary>
		/// Derived classes should implement this method to call the Guide.BeginShowStorageDeviceSelector
		/// method with the desired parameters, using the given callback.
		/// </summary>
		/// <param name="callback">The callback to pass to Guide.BeginShowStorageDeviceSelector.</param>
		protected abstract void GetStorageDevice(AsyncCallback callback);

		/// <summary>
		/// Prepares the SaveDeviceEventArgs to be used for an event.
		/// </summary>
		/// <remarks>
		/// Subclasses can override this method to change the default argument values.
		/// </remarks>
		/// <param name="args">The event arguments to be configured.</param>
		protected virtual void PrepareEventArgs(StorageDeviceEventArgs args)
		{
			args.Response = StorageDeviceEventResponse.Prompt;

			// the PlayerSaveDevice overrides us to set this to the player
			// so we'll default to null under the assumption that it's ok
			// for any player to handle the message
			args.PlayerToPrompt = null;
		}

		/// <summary>
		/// Allows the component to update itself.
		/// </summary>
		/// <param name="gameTime">The current game timestamp.</param>
		public void Update(GameTime gameTime)
		{
#if XBOX
			// make sure gamer services are available for all of our Guide methods we use			
			if (!GamerServicesDispatcher.IsInitialized)
				throw new InvalidOperationException(Strings.NeedGamerService);
#endif

#if SILVERLIGHT
            Boolean deviceIsConnected = true;
#else
			Boolean deviceIsConnected = storageDevice != null && storageDevice.IsConnected;
#endif
			if (!deviceIsConnected && deviceWasConnected)
			{
				// if the device was disconnected, fire off the event and handle result
				PrepareEventArgs(eventArgs);

				if (DeviceDisconnected != null)
					DeviceDisconnected(this, eventArgs);

				HandleEventArgResults();
			}
			else if (!deviceIsConnected)
			{
				// we use the try/catch because of the asynchronous nature of the Guide. 
				// the Guide may not be visible when we do our test, but it may open 
				// up after that point and before we've made a call, causing our Guide
				// methods to throw exceptions.
                try
                {
#if XBOX
					if (!Guide.IsVisible)
					{
#endif
                    switch (state)
                    {
                        // show the normal storage device selector
                        case StorageDevicePromptState.ShowSelector:
                            state = StorageDevicePromptState.None;
                            GetStorageDevice(storageDeviceSelectorCallback);
                            break;
                        // these actions don't apply anywhere but Xbox so we compile them out for Windows to prevent 
                        // issues with gamer services in redistributed games.
#if XBOX
							// the user cancelled the device selector, and we've decided to 
							// see if they want another chance to choose a device
							case SaveDevicePromptState.PromptForCanceled:
								ShowMessageBox(eventArgs.PlayerToPrompt, deviceOptionalTitle, promptForCancelledMessage, deviceOptionalOptions, reselectPromptCallback);
								break;

							// the user cancelled the device selector, and we've decided to
							// force them to choose again. this message is simply to inform
							// the user of that.	
							case SaveDevicePromptState.ForceCanceledReselection:
								ShowMessageBox(eventArgs.PlayerToPrompt, deviceRequiredTitle, forceCancelledReselectionMessage, deviceRequiredOptions, forcePromptCallback);
								break;

							// the device has been disconnected, and we've decided to ask
							// the user if they want to choose a new one
							case SaveDevicePromptState.PromptForDisconnected:
								ShowMessageBox(eventArgs.PlayerToPrompt, deviceOptionalTitle, promptForDisconnectedMessage, deviceOptionalOptions, reselectPromptCallback);
								break;

							// the device has been disconnected, and we've decided to force
							// the user to select a new one. this message is simply to inform
							// the user of that.
							case SaveDevicePromptState.ForceDisconnectedReselection:
								ShowMessageBox(eventArgs.PlayerToPrompt, deviceRequiredTitle, forceDisconnectedReselectionMessage, deviceRequiredOptions, forcePromptCallback);
								break;
#endif
                        default:
                            break;
                    }
#if XBOX
					}
#endif
                }

                // catch this one type of exception just to be safe
#if !SILVERLIGHT
				catch (GuideAlreadyVisibleException) { }
#else
                catch (NotSupportedException) { }
#endif
			}

			deviceWasConnected = deviceIsConnected;
		}

		/// <summary>
		/// A callback for the BeginStorageDeviceSelectorPrompt.
		/// </summary>
		/// <param name="result">The result of the prompt.</param>
		private void StorageDeviceSelectorCallback(IAsyncResult result)
		{
#if SILVERLIGHT
            throw new NotSupportedException();
#else

			//get the storage device
			storageDevice = StorageDevice.EndShowSelector(result);

			// if we got a valid device, fire off the DeviceSelected event so
			// that the game knows we have a device
			if (storageDevice != null && storageDevice.IsConnected)
			{
				if (DeviceSelected != null)
					DeviceSelected(this, null);
			}

			// if we don't have a valid device
			else
			{
				// prepare our event arguments for use
				PrepareEventArgs(eventArgs);

				// let the game know the device selector was cancelled so it
				// can tell us how to handle this
				if (DeviceSelectorCanceled != null)
					DeviceSelectorCanceled(this, eventArgs);

				// handle the result of the event
				HandleEventArgResults();
			}
#endif
		}

		/// <summary>
		/// A callback for either of the message boxes telling users they
		/// have to choose a storage device, either from cancelling the
		/// device selector or disconnecting the device.
		/// </summary>
		/// <param name="result">The result of the prompt.</param>
		private void ForcePromptCallback(IAsyncResult result)
		{
            #if SILVERLIGHT
            throw new NotSupportedException();
#else
			// just end the message and instruct the SaveDevice to show the selector
			Guide.EndShowMessageBox(result);
			state = StorageDevicePromptState.ShowSelector;
#endif
		}

		/// <summary>
		/// A callback for either of the message boxes asking the user
		/// to select a new device, either from cancelling the device
		/// seledctor or disconnecting the device.
		/// </summary>
		/// <param name="result">The result of the prompt.</param>
		private void ReselectPromptCallback(IAsyncResult result)
		{
            #if SILVERLIGHT
            throw new NotSupportedException();
#else
			int? choice = Guide.EndShowMessageBox(result);

			// get the device if the user chose the first option
			state = choice.HasValue && choice.Value == 0 ? StorageDevicePromptState.ShowSelector : StorageDevicePromptState.None;

			// fire an event for the game to know the result of the prompt
			promptEventArgs.ShowDeviceSelector = state == StorageDevicePromptState.ShowSelector;
			if (DeviceReselectPromptClosed != null)
				DeviceReselectPromptClosed(this, promptEventArgs);
#endif
		}

		/// <summary>
		/// Handles reading from the eventArgs to determine what action to take.
		/// </summary>
		private void HandleEventArgResults()
		{
            #if SILVERLIGHT
            throw new NotSupportedException();
#else
			// clear the Device reference
			storageDevice = null;


			// determine the next action...
			switch (eventArgs.Response)
			{
				// will have the manager prompt the user with the option of reselecting the storage device
				case StorageDeviceEventResponse.Prompt:
					state = deviceWasConnected
						? StorageDevicePromptState.PromptForDisconnected
						: StorageDevicePromptState.PromptForCanceled;
					break;

				// will have the manager prompt the user that the device must be selected
                case StorageDeviceEventResponse.Force:
					state = deviceWasConnected
						? StorageDevicePromptState.ForceDisconnectedReselection
						: StorageDevicePromptState.ForceCanceledReselection;
					break;

				// will have the manager do nothing
				default:
					state = StorageDevicePromptState.None;
					break;
			}
#endif
		}

		// helper method for showing guide messages
		private static void ShowMessageBox(PlayerIndex? player, String title, String text, IEnumerable<String> buttons, AsyncCallback callback)
		{
            #if SILVERLIGHT
            throw new NotSupportedException();
#else
			if (player.HasValue)
			{
				Guide.BeginShowMessageBox(player.Value, title, text, buttons, 0, MessageBoxIcon.None, callback, null);
			}
			else
			{
				Guide.BeginShowMessageBox(title, text, buttons, 0, MessageBoxIcon.None, callback, null);
			}
#endif
		}
	}
}