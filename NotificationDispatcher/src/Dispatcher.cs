using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NotificationDispatcher
{
    internal class Dispatcher
    {
        private readonly List<ScheduledNotification> scheduledNotifications = new();

        /// <summary>
        /// Добавляет сообщение в систему
        /// </summary>
        public void PushNotification(Notification notification)
        {
            DateTime scheduledTime = notification.Created;

            var lastNotificationToOtherAccounts = scheduledNotifications
                .Where(sn => sn.Notification.MessengerAccount != notification.MessengerAccount)
                .OrderByDescending(sn => sn.ScheduledDeliveryTime)
                .FirstOrDefault();

            if (lastNotificationToOtherAccounts != null)
            {
                DateTime nextAvailableTime = lastNotificationToOtherAccounts.ScheduledDeliveryTime.AddSeconds(10);
                if (nextAvailableTime > scheduledTime)
                {
                    scheduledTime = nextAvailableTime;
                }
            }

            var lastNotificationToSameAccount = scheduledNotifications
                .Where(sn => sn.Notification.MessengerAccount == notification.MessengerAccount)
                .OrderByDescending(sn => sn.ScheduledDeliveryTime)
                .FirstOrDefault();

            if (lastNotificationToSameAccount != null)
            {
                DateTime nextAvailableTime = lastNotificationToSameAccount.ScheduledDeliveryTime.AddMinutes(1);
                if (nextAvailableTime > scheduledTime)
                {
                    scheduledTime = nextAvailableTime;
                }
            }

            if (notification.Priority == NotificationPriority.Low)
            {
                var lastLowPriorityNotificationToSameAccount = scheduledNotifications
                    .Where(sn => sn.Notification.MessengerAccount == notification.MessengerAccount && sn.Notification.Priority == NotificationPriority.Low)
                    .OrderByDescending(sn => sn.ScheduledDeliveryTime)
                    .FirstOrDefault();

                if (lastLowPriorityNotificationToSameAccount != null)
                {
                    DateTime nextAvailableTime = lastLowPriorityNotificationToSameAccount.ScheduledDeliveryTime.AddHours(24);
                    if (nextAvailableTime > scheduledTime)
                    {
                        scheduledTime = nextAvailableTime;
                    }
                }
            }

            var scheduledNotification = new ScheduledNotification
            {
                Notification = notification,
                ScheduledDeliveryTime = scheduledTime
            };
            scheduledNotifications.Add(scheduledNotification);
        }

        /// <summary>
        /// Вовзращает порядок отправки сообщений
        /// </summary>
        public ReadOnlyCollection<ScheduledNotification> GetOrderedNotifications()
        {
            return scheduledNotifications.OrderBy(sn => sn.ScheduledDeliveryTime).ToList().AsReadOnly();
        }
    }
}