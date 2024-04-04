﻿using CielaDocs.AdminPanel.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using System.Collections.Generic;
using System.Text.Json;

namespace CielaDocs.AdminPanel.Extensions
{
    public static class AlertExtensions
    {
        // Adds an alert to the result using error color (red)
        public static IActionResult WithError(this IActionResult result,
                                              string message,
                                              string debugInfo = null)
        {
            return Alert(result, "danger", message, debugInfo);
        }

        // Adds an alert to the result using success color (green)
        public static IActionResult WithSuccess(this IActionResult result,
                                                string message,
                                                string debugInfo = null)
        {
            return Alert(result, "success", message, debugInfo);
        }

        // Adds an alert to the result using info color (blue)
        public static IActionResult WithInfo(this IActionResult result,
                                             string message,
                                             string debugInfo = null)
        {
            return Alert(result, "info", message, debugInfo);
        }

        // Adds an alert with a button
        public static IActionResult WithInfoActionLink(this IActionResult result,
                                                       string message,
                                                       string actionText,
                                                       string actionUrl,
                                                       string debugInfo = null)
        {
            return Alert(result, "info", message, debugInfo, actionText, actionUrl);
        }

        // Adds a new alert to the temp data dictionary
        public static void AddAlert(this ITempDataDictionary tempData,
                                    string key,
                                    Alert alert)
        {
            var alerts = tempData.GetAlerts(key);
            alerts.Add(alert);
            tempData.Remove(key);
            tempData.Add(key, JsonSerializer.Serialize(alerts));
        }

        // Gets the list of alerts from the temp data dictionary
        public static List<Alert> GetAlerts(this ITempDataDictionary tempData,
                                            string key)
        {
            object data = null;

            if (tempData.TryGetValue(key, out data))
            {
                return JsonSerializer.Deserialize<List<Alert>>((string)data);
            }

            return new List<Alert>();
        }

        private static IActionResult Alert(IActionResult result,
                                        string type,
                                        string message,
                                        string debugInfo,
                                        string actionText = null,
                                        string actionUrl = null)
        {
            return new WithAlertResult(result, type, message, debugInfo, actionText, actionUrl);
        }
    }
}
