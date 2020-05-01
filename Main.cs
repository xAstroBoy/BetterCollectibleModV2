using System;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Threading.Tasks;
using MelonLoader;
using StressLevelZero;
using StressLevelZero.Props;
using UnityEngine;
using UnityEngine.UI;

namespace Collectible_finder_v2
{
	public static class BuildInfo
	{
		public const string Name = "Collectible Finder v2"; // Name of the Mod.  (MUST BE SET)
		public const string Author = "marcocorriero && YeOldWarChap (for the original mod)"; // Author of the Mod.  (Set as null if none)
		public const string Company = null; // Company that made the Mod.  (Set as null if none)
		public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
		public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
	}

	public class CollectibleFinderV2 : MelonMod
	{
		public override void OnLevelWasInitialized(int level)
		{
		}

		public override void OnUpdate()
		{
			try
			{
				if (!isAlertShown)
				{
					MelonModLogger.Log(": Press the letter K to find all the collectibles present in the map!");
					MelonModLogger.Log(": Press the letter Q to Teleport the closest collectible to you!");
					MelonModLogger.Log(": Press the letter C to Teleport all crates to you!");
					MelonModLogger.Log(": Press the letter P to Teleport all Capsules to you!");
					MelonModLogger.Log(": Press the letter I to Teleport all ItemBalls to you!");
					MelonModLogger.Log(": Press the letter H to Toggle the line! (hidden by default)");
					MelonModLogger.Log(": Press the letter * to Increase ItemBalls and Capsule Size!");
					MelonModLogger.Log(": Press the letter - to Decrease ItemBalls and Capsule Size!");



					isAlertShown = true;
				}
				if (Input.GetKeyDown(KeyCode.D))
				{
					ToggleDebugText();
				}
				if (Input.GetKeyDown(KeyCode.C))
				{
					TeleportAllCrates();
				}
				if (Input.GetKeyDown(KeyCode.P))
				{
					TeleportAllCapsules();
				}
				if (Input.GetKeyDown(KeyCode.I))
				{
					TeleportAllItemBalls();
				}
				if (Input.GetKeyDown(KeyCode.H))
				{
					ToggleLineVisibility();
				}
				if (Input.GetKeyDown(KeyCode.K))
				{
					ResetCollectibleFinder();
				}
				if (Input.GetKeyDown(KeyCode.KeypadMinus))
				{
					DecreaseCapsuleSize();
					DecreaseItemBallSize();

				}
				if (Input.GetKeyDown(KeyCode.KeypadMultiply))
				{
					IncreaseCapsuleSize();
					IncreaseItemBallSize();
				}
				if (Input.GetKeyDown(KeyCode.Q))
				{
					TeleportClosestCollectibleToYou();
				}
				if ((Time.time - LastTimeCheck > 20))
				{
					RefreshRemovedCollectables();
					DebugLog("Starting RefreshRemovedCollectables()");
					LastTimeCheck = Time.time;
				}
			}
			catch (Exception)
			{

			}
		}


		private void ResetCollectibleFinder()
		{
			MelonModLogger.Log(": Finding Collectibles....");
			hasCreatedCollectablesList = false;
			hasCreatedCanvas = false;
			player = null;
			GameObject.DestroyObject(canvas);
			GameObject.DestroyObject(LR);
			canvas = null;
			LR = null;
			closestCollectable = null;
			allCollectables = new List<Transform>();
			CratesCollectables = new List<Transform>();
			ItemBallCollectables = new List<Transform>();
			CapsuleCollectables = new List<Transform>();
			isLineHidden = false;
			HasLineSpawned = false;
		}
		private void FindPlayer()
		{

			GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == "PlayerTrigger")
				{
					player = array[i];
				}
			}
		}

		private void UpdateDistanceUI()
		{
			if (closestCollectable == null)
			{
				collectablesText.text = "No collectables found";
				return;
			}
			collectablesText.text = string.Format("Collectable Remaining: " + allCollectables.Count, Array.Empty<object>());
		}


		private void RefreshRemovedCollectables()
		{
			if (allCollectables != null)
			{

				for (int i = allCollectables.Count - 1; i >= 0; i--)
				{
					ObjectDestructable component = allCollectables.get_Item(i).GetComponent<ObjectDestructable>();
					bool IsBroken = component != null && component._isDead;
					SaveItem component2 = allCollectables.get_Item(i).GetComponent<SaveItem>();
					bool HasBeenCollected = component2 != null && component2.saveState.bodySlot > SaveState.BodySlot.NONE;

					if (allCollectables.get_Item(i) == null || IsBroken || HasBeenCollected)
					{
						var ItemToRemove = allCollectables.get_Item(i).gameObject.transform;

						if (allCollectables.Contains(ItemToRemove))
						{
							DebugLog("RefreshRemovedCollectables Removing AllCollectables: " + ItemToRemove.name);
							allCollectables.Remove(ItemToRemove);
						}
						if (CratesCollectables.Contains(ItemToRemove))
						{
							DebugLog("RefreshRemovedCollectables Removing Crate : " + ItemToRemove.name);
							CratesCollectables.Remove(ItemToRemove);
						}
						if (CapsuleCollectables.Contains(ItemToRemove))
						{
							DebugLog("RefreshRemovedCollectables Removing Capsule :" + ItemToRemove.name);
							CapsuleCollectables.Remove(ItemToRemove);
						}
						if (ItemBallCollectables.Contains(ItemToRemove))
						{
							DebugLog("RefreshRemovedCollectables Removing ItemBall : " + ItemToRemove.name);
							ItemBallCollectables.Remove(ItemToRemove);
						}

					}
				}
				DebugLog("RefreshRemovedCollectables Method Done, calling for RefreshExistingCollectables.");
				RefreshExistingCollectables();
			}
		}

		private void DebugLog(string text)
		{
			if(isDebugMode)
			{
				MelonModLogger.Log(text);
			}
		}




		private void GetClosestCollectable()
		{
			if (allCollectables != null)
			{
				float num = float.MaxValue;
				for (int i = allCollectables.Count - 1; i >= 0; i--)
				{
					float sqrMagnitude = (allCollectables.get_Item(i).transform.position - player.transform.position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						closestCollectable = allCollectables.get_Item(i);
					}
				}
				if (allCollectables.Count == 0)
				{
					closestCollectable = null;
				}
			}
		}

		private void PerformFirstCollectablesCheck()
		{
			try
			{
				SaveItem[] array = UnityEngine.Object.FindObjectsOfType<SaveItem>();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].name.Contains("Capsule"))
					{
						AddAllCollectibles(array[i].transform);
						AddCapsuleCollectable(array[i].transform);
					}
					if (array[i].name.Contains("itemBall"))
					{
						AddAllCollectibles(array[i].transform);
						AddItemBallCollectable(array[i].transform);
					}
				}
				ObjectDestructable[] array2 = UnityEngine.Object.FindObjectsOfType<ObjectDestructable>();
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].name.Contains("Crate") && array2[j].name.Contains("Boneworks"))
					{
						AddAllCollectibles(array2[j].transform);
						AddCrateCollectable(array2[j].transform);
					}
				}
				hasCreatedCollectablesList = true;
				GetClosestCollectable();
			}
			catch (Exception)
			{
			}
		}

		private void SetCanvasPosition()
		{
			Vector3 position = player.transform.position + player.transform.forward * 1.5f - player.transform.up * 0.2f;
			canvas.transform.position = position;
			canvas.transform.rotation = player.transform.rotation;
		}

		private void CreateHUDCanvas()
		{
			try
			{

				LR = new GameObject("Line Indicator");
				LR.AddComponent<LineRenderer>();
				LR.GetComponent<LineRenderer>().startWidth = 0f;
				LR.GetComponent<LineRenderer>().endWidth = 0f;
				HasLineSpawned = true;
				canvas = new GameObject("Collectables Canvas");
				canvas.AddComponent<Canvas>();
				RectTransform component = canvas.GetComponent<RectTransform>();
				component.position = new Vector3(-52f, -1f, -24f);
				component.sizeDelta = new Vector2(1.5f, 2f);
				collectablesTextGO = new GameObject("Colectables Text");
				collectablesTextGO.transform.parent = canvas.transform;
				collectablesText = collectablesTextGO.AddComponent<Text>();
				collectablesText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
				collectablesText.text = "Wait...";
				collectablesText.fontSize = 40;
				collectablesText.alignment = TextAnchor.UpperRight;
				RectTransform component2 = collectablesTextGO.GetComponent<RectTransform>();
				component2.sizeDelta = new Vector2(800f, 800f);
				component2.localScale = Vector3.one * 0.002f;
				component2.anchorMin = new Vector2(0f, 1f);
				component2.anchorMax = new Vector2(0f, 1f);
				component2.pivot = new Vector2(1f, 1f);
				component2.localPosition = Vector3.zero;
				component2.anchoredPosition = new Vector2(component.sizeDelta.x, 0f);
				hasCreatedCanvas = true;
			}
			catch (Exception)
			{

			}
		}


		public override void OnLevelWasLoaded(int level)
		{
		}

		private void CheckForCollectibles()
		{
			try
			{
				if (!hasCreatedCollectablesList)
				{
					PerformFirstCollectablesCheck();
				}
				else
				{
					GetClosestCollectable();
				}

			}
			catch (Exception)
			{
			}
		}

		public override void OnLateUpdate()
		{
			try
			{

				if ((Time.time - LastTimeCheck2 > 2))
				{
					if (HasLineSpawned && LR != null)
					{
						UpdateLineTarget();
					}
					LastTimeCheck2 = Time.time;
				}
				InitCollectibleFinder();

			}
			catch (Exception)
			{ }
		}

		private void InitCollectibleFinder()
		{

			if (player != null)
			{
				if (!hasCreatedCanvas)
				{
					CreateHUDCanvas();
				}
				if (canvas != null)
				{
					SetCanvasPosition();
					CheckForCollectibles();
					if (hasCreatedCollectablesList)
					{
						UpdateDistanceUI();
						return;
					}
				}
			}
			else
			{
				FindPlayer();
			}
		}
		


		private void UpdateLineTarget()
		{
			try
			{
				if (HasLineSpawned && closestCollectable == null)
				{
					if (LR.GetComponent<LineRenderer>().enabled)
					{
						LR.GetComponent<LineRenderer>().enabled = false;
					}
					return;
				}
				if (closestCollectable != null)
				{
					if (!LR.GetComponent<LineRenderer>().enabled)
					{
						LR.GetComponent<LineRenderer>().enabled = true;
					}
					Vector3 position = new Vector3(player.transform.position.x, player.transform.position.y - 1f, player.transform.position.z);
					LR.GetComponent<LineRenderer>().SetPosition(0, position);
					LR.GetComponent<LineRenderer>().SetPosition(1, closestCollectable.transform.position);
				}
			}
			catch (Exception)
			{

			}
		}


		private void TeleportClosestCollectibleToYou()
		{
			try
			{
				if (closestCollectable == null)
				{
					return;
				}
				if (closestCollectable != null)
				{
					MelonModLogger.Log(": Teleporting " + closestCollectable.name + " To Your location...");


					Vector3 position = player.transform.position + player.transform.forward;
					DebugLog("Selected position is : " + position.ToString());

					closestCollectable.transform.position = position;
				}
			}
			catch (Exception)
			{

			}
		}

		private void IncreaseItemBallSize()
		{
			try
			{
				if (ItemBallCollectables == null || ItemBallCollectables.Count == 0)
				{
					return;
				}
				if (ItemBallCollectables != null)
				{
					for (int i = 0; i < ItemBallCollectables._size; i++)
					{
						var obj = ItemBallCollectables.get_Item(i);
						obj.localScale = obj.localScale * 2;
					}
				}
			}
			catch (Exception)
			{

			}
		}

		private void DecreaseItemBallSize()
		{
			try
			{
				if (ItemBallCollectables == null || ItemBallCollectables.Count == 0)
				{
					return;
				}
				if (ItemBallCollectables != null)
				{
					for (int i = 0; i < ItemBallCollectables._size; i++)
					{
						var obj = ItemBallCollectables.get_Item(i);
						obj.localScale = obj.localScale / 2;
					}
				}
			}
			catch (Exception)
			{

			}
		}
		private void IncreaseCapsuleSize()
		{
			try
			{
				if (CapsuleCollectables == null || CapsuleCollectables.Count == 0)
				{
					return;
				}
				if (CapsuleCollectables != null)
				{
					for (int i = 0; i < CapsuleCollectables._size; i++)
					{
						var obj = CapsuleCollectables.get_Item(i);
						obj.localScale = obj.localScale * 2;
					}
				}
			}
			catch (Exception)
			{

			}
		}

		private void DecreaseCapsuleSize()
		{
			try
			{
				if (CapsuleCollectables == null || CapsuleCollectables.Count == 0)
				{
					return;
				}
				if (CapsuleCollectables != null)
				{
					for (int i = 0; i < CapsuleCollectables._size; i++)
					{
						var obj = CapsuleCollectables.get_Item(i);
						obj.localScale = obj.localScale / 2;
						
					}
				}
			}
			catch (Exception)
			{

			}
		}


		private void TeleportAllCapsules()
		{
			try
			{
				if (CapsuleCollectables == null || CapsuleCollectables.Count == 0)
				{
					MelonModLogger.Log("No Capsules Detected!");
					return;
				}
				if (CapsuleCollectables != null)
				{
					for (int i = 0; i < CapsuleCollectables._size; i++)
					{
						var obj = CapsuleCollectables.get_Item(i);
						var body = obj.GetComponent<Rigidbody>();
						body.velocity = novelocity;
						DebugLog(": Teleporting " + obj.name + " To Your location...");
						MelonModLogger.Log(": Teleporting " + CapsuleCollectables.Count + " Capsules To Your location...");

						Vector3 position = player.transform.position + player.transform.forward;
						DebugLog("Selected position is : " + position.ToString());
						obj.transform.position = position;
					}
				}
			}
			catch (Exception)
			{

			}
		}

		private void TeleportAllCrates()
		{
			try
			{
				if (CratesCollectables == null || CratesCollectables.Count == 0)
				{
					MelonModLogger.Log("No Crates Detected!");
					return;
				}
				if (CratesCollectables != null)
				{
					for (int i = 0; i < CratesCollectables._size; i++)
					{
						var obj = CratesCollectables.get_Item(i);

						DebugLog(": Teleporting " + obj.name + " To Your location...");
						MelonModLogger.Log(": Teleporting " + CratesCollectables.Count + " Crates To Your location...");
						Vector3 position = player.transform.position + player.transform.forward;
						DebugLog("Selected position is : " + position.ToString());

						obj.transform.position = position;
					}
				}
			}
			catch (Exception)
			{

			}
		}

		private void TeleportAllItemBalls()
		{
			try
			{
				if (ItemBallCollectables == null || ItemBallCollectables.Count == 0)
				{
					return;
				}
				if (ItemBallCollectables != null)
				{
					for (int i = 0; i < ItemBallCollectables._size; i++)
					{
						var obj = ItemBallCollectables.get_Item(i);
						var body = obj.GetComponent<Rigidbody>();
						body.velocity = novelocity;
						DebugLog(": Teleporting " + obj.name + " To Your location...");
						MelonModLogger.Log(": Teleporting " + ItemBallCollectables.Count + " ItemBalls To Your location...");

						Vector3 position = player.transform.position + player.transform.forward;
						DebugLog("Selected position is : " + position.ToString());

						obj.transform.position = position;
					}
				}
			}
			catch (Exception)
			{

			}
		}

		private void RefreshExistingCollectables()
		{
			try
			{
				SaveItem[] array = UnityEngine.Object.FindObjectsOfType<SaveItem>();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].name.Contains("Capsule"))
					{
						AddAllCollectibles(array[i].transform);
						AddCapsuleCollectable(array[i].transform);
						DebugLog("RefreshExistingCollectables : Added new Capsule");
					}
					if (array[i].name.Contains("itemBall"))
					{
						AddAllCollectibles(array[i].transform);
						AddItemBallCollectable(array[i].transform);
						DebugLog("RefreshExistingCollectables : Added new itemball");
					}
				}
				ObjectDestructable[] array2 = UnityEngine.Object.FindObjectsOfType<ObjectDestructable>();
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].name.Contains("Crate") && array2[j].name.Contains("Boneworks"))
					{
						AddAllCollectibles(array2[j].transform);
						AddCrateCollectable(array2[j].transform);
						DebugLog("RefreshExistingCollectables : Added new Crate");
					}
				}
				DebugLog("Ending RefreshExistingCollectables()");
			}catch(Exception exc)
			{
				DebugLog("RefreshExistingCollectables exception : " + exc);
			}
		}


		private void AddCapsuleCollectable(Transform transform)
		{
			if (CapsuleCollectables == null)
			{
				CapsuleCollectables = new List<Transform>();
			}
			if (!CapsuleCollectables.Contains(transform))
			{
				CapsuleCollectables.Add(transform);
				DebugLog("CapsuleCollectables Registered " + transform.name);

			}
		}

		private void AddCrateCollectable(Transform transform)
		{
			if (CratesCollectables == null)
			{
				CratesCollectables = new List<Transform>();
			}
			if (!CratesCollectables.Contains(transform))
			{
				CratesCollectables.Add(transform);
				DebugLog("CratesCollectables Registered " + transform.name);

			}
		}

		private void AddAllCollectibles(Transform transform)
		{
			if (allCollectables == null)
			{
				allCollectables = new List<Transform>();
			}
			if (!allCollectables.Contains(transform))
			{
				allCollectables.Add(transform);
				DebugLog("AddAllCollectibles Registered " + transform.name);
			}
		}
		private void AddItemBallCollectable(Transform transform)
		{
			if (ItemBallCollectables == null)
			{
				ItemBallCollectables = new List<Transform>();
			}
			if (!ItemBallCollectables.Contains(transform))
			{
				ItemBallCollectables.Add(transform);
				DebugLog("ItemBallCollectables Registered " + transform.name);

			}
		}


		private void ToggleLineVisibility()
		{
			if (LR != null)
			{
				if (isLineHidden)
				{
					LR.GetComponent<LineRenderer>().enabled = false;
					LR.GetComponent<LineRenderer>().startWidth = 0f;
					LR.GetComponent<LineRenderer>().endWidth = 0f;
					MelonModLogger.Log("Line is now invisible!");
					isLineHidden = false;
				}
				else
				{
					LR.GetComponent<LineRenderer>().enabled = true;
					LR.GetComponent<LineRenderer>().startWidth = lineWidth;
					LR.GetComponent<LineRenderer>().endWidth = lineWidth;
					MelonModLogger.Log("Line is now visible!");
					isLineHidden = true;
				}
			}
		}

		private void ToggleDebugText()
		{

				if (isDebugMode)
				{
				isDebugMode = false;
				MelonModLogger.Log("Debug Output is now hidden!");

			}
			else
				{

				isDebugMode = true;
				MelonModLogger.Log("Debug Output is now visible!");

			}

		}

		private bool isLineHidden = true;

		private float lineWidth = 0.03f;

		private GameObject player;

		private GameObject canvas;

		private GameObject collectablesTextGO;

		private Text collectablesText;

		private bool hasCreatedCanvas;

		private bool hasCreatedCollectablesList;


		private Transform closestCollectable;
		private Vector3 novelocity = new Vector3(0, 0, 0);
		private List<Transform> allCollectables;
		private List<Transform> CratesCollectables;
		private List<Transform> ItemBallCollectables;
		private List<Transform> CapsuleCollectables;
		private GameObject LR;

		private bool isDebugMode = false;
		private bool isAlertShown;
		private float LastTimeCheck = 0;
		private float LastTimeCheck2 = 0;
		public bool HasLineSpawned;
	}
}