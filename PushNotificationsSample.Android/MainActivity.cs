using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Gms.Common;
using Android.Util;
using Android.Content;

namespace PushNotificationsSample.Droid {
	[Activity(Label = "PushNotificationsSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

		private const String NotificationChannelId = "1152";
		private const String NotificationChannelName = "Push Notifications";
		private const String NotificationChannelDescription = "Receive notifications";

		protected override void OnCreate(Bundle savedInstanceState) {
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

			if (IsPlayServicesAvailable()) {
				CreateNotificationChannel();
				LoadApplication(new App());
			}
		}
		public override void OnRequestPermissionsResult(Int32 requestCode, String[] permissions, [GeneratedEnum] Permission[] grantResults) {
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		/// <summary>
		 /// Verifies if the device has Google Play Services
		 /// </summary>
		public Boolean IsPlayServicesAvailable() {
			Int32 resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);

			if (resultCode != ConnectionResult.Success) {
				if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode)) {
					Log.Error("MAIN", GoogleApiAvailability.Instance.GetErrorString(resultCode));
				} else {
					Log.Error("MAIN", "Device not supported.");
					Finish();
				}

				return false;
			} else {
				return true;
			}
		}

		/// <summary>
		/// Overriding this method to handle user interaction with push notifications
		/// </summary>
		protected override void OnNewIntent(Intent intent) {
			base.OnNewIntent(intent);

			String extraInfo = String.Empty;

			if (intent.Extras != null) {
				foreach (String key in intent.Extras.KeySet()) {
					String value = intent.Extras.GetString(key);

					if (key == "ExtraInfo" && !String.IsNullOrEmpty(value)) {
						extraInfo = value;
					}
				}
			}

			//NavigationExtension.HandlePushNotificationNavigation(extraInfo);
		}

		/// <summary>
		/// Creates a Notification Channel which is only applicable to API 26/8.0.0/Oreo
		/// </summary>
		public void CreateNotificationChannel() {
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {

				NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

				NotificationChannel notificationChannel = new NotificationChannel(NotificationChannelId, NotificationChannelName, NotificationImportance.High) {
					Description = NotificationChannelDescription
				};

				notificationManager.CreateNotificationChannel(notificationChannel);
			}
		}
	}
}
