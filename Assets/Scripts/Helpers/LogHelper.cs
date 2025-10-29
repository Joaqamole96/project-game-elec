using System;
using UnityEngine;

namespace Helpers
{
    internal static class LogHelper
    {
        public static void Declare(object sender, object attribute) =>
            Debug.Log($"[DECL] {sender.GetType().Name}: {attribute.GetType().Name}={attribute}");

        public static void Inform(object sender, string message) =>
            Debug.Log($"[INFO] {sender.GetType().Name}: {message}");

        public static void Success(object sender, string message) =>
            Debug.Log($"[GOOD] {sender.GetType().Name}: {message}");

        public static void Warn(object sender, string message) =>
            Debug.LogWarning($"[WARN] {sender.GetType().Name}: {message}");

        public static Exception Failure(object sender, string message)
        {
            return new Exception($"[FAIL] {sender.GetType().Name}: {message}");
        }
    }
}