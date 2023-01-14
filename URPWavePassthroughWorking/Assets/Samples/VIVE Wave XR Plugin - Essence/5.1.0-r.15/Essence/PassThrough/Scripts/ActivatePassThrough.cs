// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using Wave.Native;
using Wave.Essence.Hand;
using Wave.Essence.Events;

namespace Wave.Essence.Samples.PassThrough
{
	public class ActivatePassThrough : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Samples.PassThrough.ActivatePassThrough";
		void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}

		bool showUnderlay = false;
		public void ActivateUnderlay()
		{
			showUnderlay = !showUnderlay;
			var result = Interop.WVR_ShowPassthroughUnderlay(showUnderlay);
			DEBUG("ActivateUnderlay() " + showUnderlay + ", result: " + result);
		}
		bool showOverlay = false;
		public void ActivateOverlay()
		{
			showOverlay = !showOverlay;
			var result = Interop.WVR_ShowPassthroughOverlay(showOverlay);
			DEBUG("ActivateUnderlay() " + showOverlay + ", result: " + result);
		}

		private void Update()
		{
			if (WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_Menu))
				ActivateOverlay();
		}
		private bool mEnabled = false;
		void OnEnable()
		{
			if (!mEnabled)
			{
				GeneralEvent.Listen(HandManager.HAND_STATIC_GESTURE, OnStaticGesture);
				mEnabled = true;
			}
		}
		void OnDisable()
		{
			if (mEnabled)
			{
				GeneralEvent.Remove(HandManager.HAND_STATIC_GESTURE, OnStaticGesture);
				mEnabled = false;
			}
		}

		private void OnStaticGesture(params object[] args)
		{
			var hand = (HandManager.HandType)args[0];
			var gesture = (HandManager.GestureType)args[1];
			DEBUG("OnStaticGesture() " + hand + ", " + gesture);

			if (hand == HandManager.HandType.Left && gesture == HandManager.GestureType.Palm_Pinch)
				ActivateOverlay();
		}
	}
}
