using System;
using UnityEngine;
using ProjectS.Protocol;

namespace ProjectS
{
    public static partial class NetworkProcess
    {
        public static void GetShelterData(Action<getShelterData.result> onResult)
        {
            var req = new getShelterData()
            {
            };

            Managers.Net.Request<getShelterData.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void StartExtendShelter(bool bUseCash, Action<doExtendToShelter.result> onResult)
        {
            var req = new doExtendToShelter()
            {
                goods_type = bUseCash ? 1 : 0,
            };

            Managers.Net.Request<doExtendToShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CompleteExtendShelter(Action<completeExtendShelter.result> onResult)
        {
            var req = new completeExtendShelter()
            {
            };

            Managers.Net.Request<completeExtendShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void ExtendShelterImmediately(Action<doImmediateExtendToShelter.result> onResult)
        {
            var req = new doImmediateExtendToShelter()
            {
            };

            Managers.Net.Request<doImmediateExtendToShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void StartMigrateShelter(bool bUseCash, Action<doRepairToShelter.result> onResult)
        {
            var req = new doRepairToShelter()
            {
                goods_type = bUseCash ? 1 : 0,
            };

            Managers.Net.Request<doRepairToShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CompleteMigrateShelter(Action<doMoveToShelter.result> onResult)
        {
            var req = new doMoveToShelter()
            {
            };

            Managers.Net.Request<doMoveToShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void MigrateShelterImmediately(Action<doImmediateRepairToShelter.result> onResult)
        {
            var req = new doImmediateRepairToShelter()
            {
            };

            Managers.Net.Request<doImmediateRepairToShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CancelExtendShelter(Action<cancelToExtendShelter.result> onResult)
        {
            var req = new cancelToExtendShelter()
            {
            };

            Managers.Net.Request<cancelToExtendShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CancelMigrationShelter(Action<cancelToRepairShelter.result> onResult)
        {
            var req = new cancelToRepairShelter()
            {
            };

            Managers.Net.Request<cancelToRepairShelter.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void GetDeviceData(Action<getDeviceData.result> onResult)
        {
            var req = new getDeviceData()
            {
            };

            Managers.Net.Request<getDeviceData.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void SetupDevice(int nDeviceDataID, int nShelterLevelID, bool bUseCach, Action<doDeviceSetup.result> onResult)
        {
            var req = new doDeviceSetup()
            {
                device_id = nDeviceDataID,
                shelter_level_id = nShelterLevelID,
                goods_type = bUseCach ? 1 : 0,
            };

            Managers.Net.Request<doDeviceSetup.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CancelSetupDevice(long lDeviceKey, int nShelterLevelID, Action<cancelToDevice.result> onResult)
        {
            var req = new cancelToDevice()
            {
                device_key = lDeviceKey,
                shelter_level_id = nShelterLevelID,
            };

            Managers.Net.Request<cancelToDevice.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CompleteSetupDevice(long lDeviceKey, int nShelterLevelID, Action<completeToDevice.result> onResult)
        {
            var req = new completeToDevice()
            {
                device_key = lDeviceKey,
                shelter_level_id = nShelterLevelID,
            };

            Managers.Net.Request<completeToDevice.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }


        public static void SetupDeviceImmediately(long lDeviceKey, Action<doImmediateDevice.result> onResult)
        {
            var req = new doImmediateDevice()
            {
                device_key = lDeviceKey,
            };

            Managers.Net.Request<doImmediateDevice.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void RemoveDevice(long lDeviceKey, int nShelterLevelID, Action<removeToDevice.result> onResult)
        {
            var req = new removeToDevice()
            {
                device_key = lDeviceKey,
                shelter_level_id = nShelterLevelID,
            };

            Managers.Net.Request<removeToDevice.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void MoveDevice(int nShelterLevelID, int nMoveShelterLevelID, Action<doMoveToDevice.result> onResult)
        {
            var req = new doMoveToDevice()
            {
                shelter_level_id = nShelterLevelID,
                move_shelter_level_id = nMoveShelterLevelID,
            };

            Managers.Net.Request<doMoveToDevice.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void StartCraftCombine(long lDeviceKey, int nCraftID, int nCount, Action<doCraftToCombineDeviceRecipe.result> onResult)
        {
            var req = new doCraftToCombineDeviceRecipe()
            {
                device_key = lDeviceKey,
                craft_id = nCraftID,
                craft_cnt = nCount,
            };

            Managers.Net.Request<doCraftToCombineDeviceRecipe.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void StartCraftRefine(long lDeviceKey, int nCraftID, int nCount, bool bCash, Action<doCraftToRefineDeviceRecipe.result> onResult)
        {
            var req = new doCraftToRefineDeviceRecipe()
            {
                goods_type = bCash ? 1 : 0,
                device_key = lDeviceKey,
                craft_id = nCraftID,
                craft_cnt = nCount,
            };

            Managers.Net.Request<doCraftToRefineDeviceRecipe.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void StartCraftSmeltUsingItem(long lDeviceKey, int nCraftID, int nCount, Action<doCraftToSmeltDeviceRecipe.result> onResult)
        {
            var req = new doCraftToSmeltDeviceRecipe()
            {
                device_key = lDeviceKey,
                craft_id = nCraftID,
                craft_cnt = nCount,
            };

            Managers.Net.Request<doCraftToSmeltDeviceRecipe.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void StartCraftSmeltUsingCash(long lDeviceKey, int nCraftID, int nCount, Action<doCraftToSmeltDeviceCashRecipe.result> onResult)
        {
            var req = new doCraftToSmeltDeviceCashRecipe()
            {
                device_key = lDeviceKey,
                craft_id = nCraftID,
                craft_cnt = nCount,
            };

            Managers.Net.Request<doCraftToSmeltDeviceCashRecipe.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void StartCraftSmeltUsingGoods(long lDeviceKey, int nCraftID, int nCount, bool bUseCash, Action<doCraftToSmeltDeviceGoodsRecipe.result> onResult)
        {
            var req = new doCraftToSmeltDeviceGoodsRecipe()
            {
                device_key = lDeviceKey,
                craft_id = nCraftID,
                craft_cnt = nCount,
                goods_type = bUseCash ? 1 : 0,
            };

            Managers.Net.Request<doCraftToSmeltDeviceGoodsRecipe.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CancelCraftCombine(long lDeviceKey, Action<cancelToCraftCombineDevice.result> onResult)
        {
            var req = new cancelToCraftCombineDevice()
            {
                device_key = lDeviceKey,
            };

            Managers.Net.Request<cancelToCraftCombineDevice.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CancelCraftRefine(long lDeviceKey, Action<cancelToCraftRefineDevice.result> onResult)
        {
            var req = new cancelToCraftRefineDevice()
            {
                device_key = lDeviceKey,
            };

            Managers.Net.Request<cancelToCraftRefineDevice.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void CompleteCraft(long lDeviceKey, Action<completeToCraft.result> onResult)
        {
            var req = new completeToCraft()
            {
                device_key = lDeviceKey,
            };

            Managers.Net.Request<completeToCraft.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }


        public static void CompleteCraftImmediately(long lDeviceKey, Action<doImmediateCraft.result> onResult)
        {
            var req = new doImmediateCraft()
            {
                device_key = lDeviceKey,
            };

            Managers.Net.Request<doImmediateCraft.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void DoGoodsDeviceFarming(long lDeviceKey, Action<doGoodsDeviceFarming.result> onResult)
        {
            var req = new doGoodsDeviceFarming()
            {
                device_key = lDeviceKey,
            };

            Managers.Net.Request<doGoodsDeviceFarming.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void DoGoodsDevicesFarming(int nDeviceType, Action<doGoodsDevicesFarming.result> onResult)
        {
            var req = new doGoodsDevicesFarming()
            {
                device_type = nDeviceType,
            };

            Managers.Net.Request<doGoodsDevicesFarming.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }

        public static void TrapActive(long[] arrDeviceKey, bool bUseCach, Action<trapIsActive.result> onResult)
        {
            var req = new trapIsActive()
            {
                device_keys = arrDeviceKey,
                goods_type = bUseCach ? 1 : 0,
            };

            Managers.Net.Request<trapIsActive.result>(req, (resp) => onResult?.Invoke(resp?.CheckCommonStatus()));
        }
    }
}