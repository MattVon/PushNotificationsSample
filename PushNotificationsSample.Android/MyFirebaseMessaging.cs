using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Util;
using Firebase.Messaging;
using PushNotificationsSample.Constants;
using PushNotificationsSample.Models;
using WindowsAzure.Messaging;

namespace PushNotificationsSample.Droid {
	[Service]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class MyFirebaseMessaging : FirebaseMessagingService {

		private const String NotificationChannelId = "1152";
		private NotificationManager notificationManager;

		public override void OnNewToken(String token) => SendTokenToAzure(token);

		/// <summary>
		/// Sends the token to Azure for registration against the device
		/// </summary>
		private void SendTokenToAzure(String token) {
			try {
				NotificationHub hub = new NotificationHub(AzureConstants.NotificationHubName, AzureConstants.ListenConnectionString, Application.Context);

				Task.Run(() => hub.Register(token, new String[] { }));
			} catch (Exception ex) {
				Log.Error("ERROR", $"Error registering device: {ex.Message}");
			}
		}

		/// <summary>
		/// When the app receives a notification, this method is called
		/// 
		/// </summary>
		public override void OnMessageReceived(RemoteMessage remoteMessage) {

			// Expected JSON
			//	{
			//		"data": {
			//			"title": "Postman Test",
			//			"body": "Using Postman",
			//			"extraInfo": "Lalalala"
			//		}
			//	}

			if (remoteMessage.Data.Count >= 1) {
				Boolean hasTitle = remoteMessage.Data.TryGetValue("title", out String title);
				Boolean hasBody = remoteMessage.Data.TryGetValue("body", out String body);
				Boolean hasExtraInfo = remoteMessage.Data.TryGetValue("extraInfo", out String extraInfo);

				PushNotification push = new PushNotification {
					Title = hasTitle ? title : String.Empty,
					Body = hasBody ? body : String.Empty,
					ExtraInfo = hasExtraInfo ? extraInfo : String.Empty
				};

				SendNotification(push);
			}
		}
		/// <summary>
		/// Handles the notification to ensure the Notification manager is updated to alert the user
		/// </summary>
		private void SendNotification(PushNotification push) {
			// Create relevant non-repeatable Id to allow multiple notifications to be displayed in the Notification Manager
			Int32 notificationId = Int32.Parse(DateTime.Now.ToString("MMddHHmmsss"));

			Intent intent = new Intent(this, typeof(MainActivity));
			intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
			intent.PutExtra("Area", push.Area);
			intent.PutExtra("ExtraInfo", push.ExtraInfo);

			PendingIntent pendingIntent = PendingIntent.GetActivity(this, notificationId, intent, PendingIntentFlags.UpdateCurrent);
			notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

			// Set BigTextStyle for expandable notifications
			NotificationCompat.BigTextStyle bigTextStyle = new NotificationCompat.BigTextStyle();
			bigTextStyle.SetSummaryText(push.Body);
			bigTextStyle.SetSummaryText(String.Empty);

			Int64 timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

			Notification notification = new NotificationCompat.Builder(this, NotificationChannelId)
			.SetSmallIcon(Resource.Drawable.ic_launcher)
			.SetContentTitle(push.Title)
			.SetContentText(push.Body)
			.SetStyle(bigTextStyle)
			.SetPriority(NotificationCompat.PriorityHigh)
			.SetWhen(timestamp)
			.SetShowWhen(true)
			.SetContentIntent(pendingIntent)
			.SetAutoCancel(true)
			.Build();

			notificationManager.Notify(notificationId, notification);

			notificationManager.Notify(notificationId, notification);
		}
	}
}
