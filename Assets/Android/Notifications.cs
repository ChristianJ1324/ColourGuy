using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

public class Notifications : MonoBehaviour
{
    public bool havePerm;

    private string notiPerm = "android.permission.POST_NOTIFICATIONS";

    private void Start()
    {
        // coroutine to check for permission
        StartCoroutine(RequestPermission());
    }

    public IEnumerator RequestPermission()
    {
        // Setup
        var req = new Unity.Notifications.Android.PermissionRequest();

        Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");

        // Requesting Permission
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending) yield return null;

        havePerm = request.Status == PermissionStatus.Allowed;

        // Requesting API permission
        if (Permission.HasUserAuthorizedPermission(notiPerm))
        {
            havePerm = true; // already have permission don't need to ask
        }
        else
        {
            // set up callbacks
            PermissionCallbacks callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += (string _name) => { havePerm = true; };
            callbacks.PermissionDenied += (string _name) => { havePerm = false; };
            //callbacks.PermissionDeniedAndDontAskAgain += (string _name) => { havePerm = false; };

            // ask permission
            Permission.RequestUserPermission(notiPerm, callbacks);
        }

        // Initialize Notification
        var group = new AndroidNotificationChannelGroup()
        {
            Id = "Main",
            Name = "Main notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannelGroup(group);

        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "General notifications",
            Group = "Main", // Must be same as id of previously registeed group
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    public void SendNotification(string _title, string _text)
    {
        SendNotification(_title, _text, System.DateTime.Now.AddSeconds(1));
    }

    public void SendNotification(string _title, string _text, DateTime _time)
    {
        var notification = new AndroidNotification();
        notification.Title = _title;
        notification.Text = _text;
        notification.FireTime = _time;

        AndroidNotificationCenter.SendNotification(notification, "channel_id");

        // debug message
        Debug.Log("Notifaction scheduled for" + _time.ToString());
    }
}
