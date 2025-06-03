using HarmonyLib;
using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

using StoreManager = MonoBehaviourPublicObInObUnique;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using System.Reflection;
using Il2CppInterop.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using VRC.Core;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;

internal class Helper
{
	public static Il2CppReferenceArray<IProduct> GetAllProducts()
	{
		var products = StoreManager.field_Public_Static_MonoBehaviourPublicObInObUnique_0.field_Internal_ObjectPublic661ILDi2St740Li10_0.field_Private_Dictionary_2_String_InterfacePublicAbstractIComparableIEquatable1ObfStObStBoOb1StTeILStUnique_0.Values;
		var iproducts = new Il2CppReferenceArray<IProduct>(products.Count);
		var i = 0;
		foreach (var product in products)
		{
			var iproduct = product.Cast<IProduct>();
			NMod.VM.Logger.LogInfo($"Product: {iproduct.Name} ({iproduct.Description})");
			product.prop_VRCPlayerApi_0 = Networking.LocalPlayer;
			iproducts[i] = iproduct;
			// var stub = new UdonProductStub(iproduct.ID, iproduct.Name, iproduct.Description, Networking.LocalPlayer);
			// var stub = new Object1PublicIProductIEquatable1IProductIComparableIEquatable1ObfSt1TeObLi1TeStVRObUnique();
			// stub.
			// stub.
			// iproducts[i] = stub.Cast<IProduct>();
			++i;
		}
		NMod.VM.Logger.LogInfo($"Products: {i}");
		return iproducts;
	}

// 	public static void Patch()
// 	{
// 		NMod.VM.Logger.LogInfo("Patch list: " + Store._listPurchases?.ToString());
// 		Store._listPurchases = new Action<IUdonEventReceiver, VRCPlayerApi>((IUdonEventReceiver receiver, VRCPlayerApi player) => {
// 			NMod.VM.Logger.LogInfo("List Purchases action");
// 			if (player?.isLocal != true)
// 			{
// 				return;
// 			}
// 			// Store.Li
// 			receiver.RunEvent("_onListPurchases",
// 				new Il2CppSystem.ValueTuple<string, Il2CppReferenceArray<IProduct>>("result", Helper.GetAllProducts()),
// 				new Il2CppSystem.ValueTuple<string, VRCPlayerApi>("result", Networking.LocalPlayer)
// 			);
// 		});

// 		// Store.
// 		// var method = (IntPtr)typeof(UdonBehaviour)
// 		// 	.GetField(
// 		// 		"NativeMethodInfoPtr_SetEventVariable_Private_Void_String_String_T_0",
// 		// 		BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
// 		// var methodInfo = new Il2CppSystem.Reflection.MethodInfo(
// 		// 	IL2CPP.il2cpp_method_get_object(method, Il2CppClassPointerStore<UdonBehaviour>.NativeClassPtr))
// 		// 	.MakeGenericMethod(new Il2CppReferenceArray<Il2CppSystem.Type>([Il2CppSystem.Type.internal_from_handle(IL2CPP.il2cpp_class_get_type(Il2CppClassPointerStore<Il2CppReferenceArray<IProduct>>.NativeClassPtr))]));

// 		// var Pointer = IL2CPP.il2cpp_method_get_from_reflection(IL2CPP.Il2CppObjectBaseToPtrNotNull(methodInfo));

// 		// NMod.VM.Logger.LogInfo($"Found SetEventVariable<IProduct[]> at 0x{Pointer:X}");

// 		// unsafe
// 		// {
// 		// 	delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, void> detour = &Setter;
// 		// 	var hook = new NativeHook<SetterDelegate>(Pointer, (IntPtr)detour);
// 		// 	hook.Attach();
// 		// 	original = hook.Trampoline;
// 		// }
// 	}

// // 	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
// // 	internal delegate void SetterDelegate(IntPtr @this, IntPtr eventName, IntPtr symbolName, IntPtr value);

// // 	internal static SetterDelegate original;

// // 	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
// // 	static void Setter(IntPtr @this, IntPtr eventName, IntPtr symbolName, IntPtr value)
// // 	{
// // 		try
// // 		{
// // 			var this_ = new UdonBehaviour(@this);
// // 			var eventName_ = (string)new Il2CppSystem.String(eventName);
// // 			var symbolName_ = (string)new Il2CppSystem.String(symbolName);
// // 			var value_ = new Il2CppSystem.Object(value);

// // 			NMod.VM.Logger.LogInfo($"SetEventVariable: {this_.gameObject.name} {eventName}, {symbolName} -> {value}");
// // 			if (eventName_ == "_onPurchasesLoaded" && symbolName_ == "result")
// // 			{
// // 				var products = Helper.GetAllProducts();
// // 				original(@this, eventName, symbolName, products.Pointer);
// // 			}
// // 			else
// // 			{
// // 				original(@this, eventName, symbolName, value);
// // 			}
// // 		}
// // 		catch (Exception ex)
// // 		{
// // 			MelonLogger.Error("issue", ex);
// // 		}
// // 	}
}

[HarmonyPatch(typeof(UdonBehaviour), nameof(UdonBehaviour.InitializeUdonContent))]
class EconomyUdonBehaviourPath
{
	static readonly List<UdonBehaviour> _onPurchaseConfirmed = [];
	static readonly List<UdonBehaviour> _onPurchasesLoaded = [];
	static void Postfix(UdonBehaviour __instance)
	{
		if (__instance._eventTable.ContainsKey("_onPurchaseConfirmed"))
		{
			NMod.VM.Logger.LogInfo($"_onPurchaseConfirmed: {__instance.gameObject.name}");
			_onPurchaseConfirmed.Add(__instance);
		}
		if (__instance._eventTable.ContainsKey("_onPurchasesLoaded"))
		{
			NMod.VM.Logger.LogInfo($"_onPurchasesLoaded: {__instance.gameObject.name}");
			_onPurchasesLoaded.Add(__instance);
		}
		// foreach (var product in Helper.GetAllProducts())
		// {
		// 	__instance.RunEvent("_onPurchaseConfirmed",
		// 		new Il2CppSystem.ValueTuple<string, IProduct>("result", product),
		// 		new Il2CppSystem.ValueTuple<string, VRCPlayerApi>("player", Networking.LocalPlayer),
		// 		new Il2CppSystem.ValueTuple<string, bool>("player", false)
		// 	);
		// }
	}
	public static void PurchaseAll()
	{
		var products = Helper.GetAllProducts();
		// var store = Il2Cpp.MonoBehaviourPublicObInObUnique.field_Public_Static_MonoBehaviourPublicObInObUnique_0;
		// foreach (var product in products) {
		// 	// store.field_Internal_ObjectPublic1ILDi2StObLi1HaObUnique_0.
		// 	// Inspect(typeof(Il2CppVRC.Core.API))
		// 	// ApiPurchase
		// 	// ApiProduct
		// 	// StoreManager
		// 	// ApiProduct
		// 	// ObjectPublicICloneableSoTySoUnique,
		// 	// InterfacePublicAbstractSt1TeOb1DaStBoDaVoUnique
		// 	// Il2Cpp.MonoBehaviour1PublicObInIE1EcBoBoUnique
		// }

		NMod.VM.Logger.LogInfo("_onPurchasesLoaded");
		foreach (var behavior in _onPurchasesLoaded)
		{
			NMod.VM.Logger.LogInfo($"Sending purchases to {behavior.gameObject.name}");
			behavior.RunEvent("_onPurchasesLoaded",
				new Il2CppSystem.ValueTuple<string, Il2CppReferenceArray<IProduct>>("result", products),
				new Il2CppSystem.ValueTuple<string, VRCPlayerApi>("player", Networking.LocalPlayer)
			);
		}

		NMod.VM.Logger.LogInfo("_onPurchaseConfirmed");
		foreach (var behavior in _onPurchaseConfirmed)
		{
			NMod.VM.Logger.LogInfo($"Sending purchases to {behavior.gameObject.name}");
			foreach (var product in products)
			{
				// product.Buyer = Networking.LocalPlayer;
				behavior.RunEvent("_onPurchaseConfirmed",
					new Il2CppSystem.ValueTuple<string, IProduct>("result", product),
					new Il2CppSystem.ValueTuple<string, VRCPlayerApi>("player", Networking.LocalPlayer),
					new Il2CppSystem.ValueTuple<string, bool>("purchasedNow", true)
				);
			}
		}
	}

	public static void OnLeavingWorld() {
		_onPurchaseConfirmed.Clear();
		_onPurchasesLoaded.Clear();
	}
}

[HarmonyPatch(typeof(Store), nameof(Store.ListPurchases), typeof(IUdonEventReceiver), typeof(VRCPlayerApi))]
class EconomyListPurchasesPatch
{
	static bool Prefix(IUdonEventReceiver eventReceiver, VRCPlayerApi purchaser)
	{
		NMod.VM.Logger.LogInfo($"EconomyListPurchasesPatch: {purchaser.displayName}");

		if (purchaser?.isLocal != true)
		{
			return true;
		}
		// Store.Li
		eventReceiver.RunEvent("_onListPurchases",
			new Il2CppSystem.ValueTuple<string, Il2CppReferenceArray<IProduct>>("result", Helper.GetAllProducts()),
			new Il2CppSystem.ValueTuple<string, VRCPlayerApi>("result", Networking.LocalPlayer)
		);
		return false;
	}
}

[HarmonyPatch(typeof(Store), nameof(Store.DoesAnyPlayerOwnProduct), typeof(IProduct))]
class EconomyDoesAnyPlayerOwnProductPatch
{
	static bool Prefix(ref bool __result)
	{
		NMod.VM.Logger.LogInfo($"DoesAnyPlayerOwnProduct");
		__result = true;
		return false;
	}
}

[HarmonyPatch(typeof(Store), nameof(Store.DoesPlayerOwnProduct), typeof(VRCPlayerApi), typeof(IProduct))]
class EconomyDoesPlayerOwnProductPatch
{
	static bool Prefix(VRCPlayerApi player, IProduct product, ref bool __result)
	{
		NMod.VM.Logger.LogInfo($"DoesPlayerOwnProduct: {player.displayName} -> {product.Name} ({product.ID})");
		return !(__result = player?.isLocal == true);
	}
}

[HarmonyPatch(typeof(Store), nameof(Store.GetPlayersWhoOwnProduct), typeof(IProduct))]
class EconomyGetPlayersWhoOwnProductPatch
{
	static void Postfix(IProduct product, ref Il2CppReferenceArray<VRCPlayerApi> __result)
	{
		NMod.VM.Logger.LogInfo($"GetPlayersWhoOwnProduct: {product.Name} ({product.ID})");
		__result = new Il2CppReferenceArray<VRCPlayerApi>(1);
		__result[0] = Networking.LocalPlayer;
	}
}

[HarmonyPatch(typeof(Store), nameof(Store.ListProductOwners), typeof(IUdonEventReceiver), typeof(IProduct))]
class EconomyListProductOwnersPatch
{
	static bool Prefix(IUdonEventReceiver eventReceiver, IProduct product)
	{
		VRC.Economy.Store.ListProductOwners(null, new VRC.Economy.UdonProduct().Cast<VRC.Economy.IProduct>());
		NMod.VM.Logger.LogInfo($"ListProductOwners: {product.Name} ({product.ID})");
		eventReceiver.RunEvent("_onListProductOwners",
			new Il2CppSystem.ValueTuple<string, IProduct>("result", product),
			new Il2CppSystem.ValueTuple<string, Il2CppStringArray>("owners", new[] { Networking.LocalPlayer.displayName })
		);
		return false;
	}
}

[HarmonyPatch(typeof(Store), nameof(Store.DoesPlayerOwnProduct), typeof(VRCPlayerApi), typeof(IProduct))]
class EconomyPatch
{
	static bool Prefix(ref VRCPlayerApi player, IProduct product, ref bool __result)
	{
		// NMod.VM.Logger.LogInfo("bool Store.DoesPlayerOwnProduct(Il2CppVRC.SDKBase.VRCPlayerApi param_1, Il2CppVRC.Economy.IProduct param_2)");
		return !(__result = player.isLocal);
	}
}

[HarmonyPatch(typeof(MonoBehaviourPublicObInObUnique), nameof(MonoBehaviourPublicObInObUnique.Method_Private_Boolean_VRCPlayerApi_IProduct_PDM_0))]
class StoreManagerOwnsProductPatch
{
	static bool Prefix(MonoBehaviourPublicObInObUnique __instance, ref bool __result, VRC.SDKBase.VRCPlayerApi __0, VRC.Economy.IProduct __1)
	{
		// NMod.VM.Logger.LogInfo("bool Il2Cpp.MonoBehaviourPublicObInObUnique::Method_Private_Boolean_VRCPlayerApi_IProduct_PDM_0(Il2CppVRC.SDKBase.VRCPlayerApi param_1, Il2CppVRC.Economy.IProduct param_2)");
		return !(__result = __0.isLocal);
	}
}