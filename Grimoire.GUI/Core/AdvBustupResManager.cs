using AssetsTools.NET.Extra;
using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Models.RF5;
using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grimoire.GUI.Core
{
    public class AdvBustupResManager
    {
        public static float[] BustupScaleTable; // 0x00
        private Dictionary<int, int> LoaderDictionary; // 0x18
        private BustupDataList? BustupDataList; // 0x20

        //CheckInit loads everything needed
        public AdvBustupResManager()
        {
            BustupDataList = Loader.LoadID<BustupDataList>((int)Master.UIDATA_BUSTUPDATATABLE);
#if !DEBUG
            if (BustupDataList == null)
                throw new Exception("Can't find `UIDATA_BUSTUPDATATABLE`");
#endif
        }
        public int GetLoaderID(NPCID npcID, int poseID, int costumeID)
        {
            //CheckInit is called here and is how the BU list is init
            //Will call in constructor instead
            //Use `DataLoader_Int32Enum__object___Get` in CheckInit
            //Use NPCID to index

            //v14
            var unk1 = 0;
            var result = 0;
            //v16
            var unk4 = 1;

            //v28
            var unk2 = 0;
            //v29
            var unk3 = -1;
            switch (npcID)
            {
                case NPCID.Baby:
                    {
                        //var childParameters = SV.childParameters
                        //if (BustupChildParameter.get_Gender(childParameters[0]) == 1)
                        //charID = 30
                        //else
                        //charID = 31
                        //...
                    }
                    break;
                default:
                    {
                        var charID = (int)CheckChangeBUSTUPID(npcID, poseID, costumeID);
                        unk1 = 0;
                        unk4 = 0;
                        var buData = BustupDataList.Datas[charID];
                        var size = buData.Data.Count;

                        
                        while (true)
                        {
                            var data = buData.Data[unk2];
                            if (data.Val1 != unk1 || data.Val2 != unk4)
                            {
                                result = unk3;
                                size = buData.Data.Count;
                                unk3 = result;
                                if (++unk2 >= size)
                                    return result;
                            }
                            else
                            {
                                var poseNo = data.PoseNo;
                                result = data.LoadID;
                                if (poseNo == poseID)
                                    break;
                                size = buData.Data.Count;
                                unk3 = result;
                                if (++unk2 >= size)
                                    return result;
                            }
                        }
                    }
                    break;
            }
            if (result < 0)
                return unk3;
            return result;
        }
        
        public BUSTUPID CheckChangeBUSTUPID(NPCID npcID, int poseID, int costumeID)
        {
            if (npcID == NPCID.None && costumeID != 1)
                return BUSTUPID.Max;
            if (npcID != NPCID.Alice || costumeID == 1)
            {
                switch (npcID)
                {
                    case NPCID.Priscilla:
                        {
                            if (costumeID != 1)
                                return BUSTUPID.Priscilla2;
                            if (poseID != -1)
                                //NpcDataManager.IsPriscillaChangedStyle();
                                return BUSTUPID.Priscilla2;
                        }
                        break;
                    case NPCID.Fuqua:
                        {
                            //This seems to return a choice of either 39 or 45??
                            return (BUSTUPID)((costumeID - 2 >= 3) ? 5 : 45);
                        }
                    case NPCID.Ludmila:
                        {
                            return (BUSTUPID)((costumeID != 2) ? (int)npcID : 40);
                        }
                    case NPCID.Martin:
                        {
                            return (BUSTUPID)((costumeID == 1) ? (int)npcID : 43);
                        }
                    case NPCID.Lyka:
                        {
                            return (BUSTUPID)((costumeID == 1) ? (int)npcID : 41);
                        }
                    case NPCID.Lucas:
                        {
                            if (costumeID != 1)
                            {
                                return (BUSTUPID)npcID;
                            }
                            //if (AdvBustupResManager.IsLucusWearingGlasses())
                            //return (BUSTUPID)npcID;
                            return (BUSTUPID)42;
                        }

                    case NPCID.Rivia:
                        {
                            //if (LovePointManager.GetLoveLv(22, 0LL) <= 4)
                            return (BUSTUPID)npcID;
                            //else
                            //return BUSTUPID.Rivia2;
                        }
                    default:
                        return (BUSTUPID)npcID;
                }
                return (BUSTUPID)npcID;
            }
            return BUSTUPID.FirstBaby;
        }
    }
}