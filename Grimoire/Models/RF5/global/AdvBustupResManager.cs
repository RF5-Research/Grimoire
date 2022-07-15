using AssetsTools.NET.Extra;
using Grimoire.Models.RF5.Define;
using Grimoire.Models.RF5.Loader.ID;
using System.Collections.Generic;
using System.ComponentModel;

namespace Grimoire.Models.RF5
{
    public class AdvBustupResManager : INotifyPropertyChanged
    {
        public static float[] BustupScaleTable { get; set; } // 0x00
        private Dictionary<int, int> LoaderDictionary { get; set; } // 0x18
        private BustupDataList? BustupDataList { get; set; } // 0x20

        //CheckInit loads everything needed
        public AdvBustupResManager(int bustupDataTableID)
        {
            var am = new AssetsManager();
            BustupDataList = AssetsLoader.LoadID<BustupDataList>(bustupDataTableID, am);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int GetLoaderID(NPCID npcID, int poseID, int costumeID)
        {
            //CheckInit is called here and is how the BU list is init
            //Will call in constructor instead
            //Use `DataLoader_Int32Enum__object___Get` in CheckInit
            //Use NPCID to index

            //v14
            var val1 = 0;
            var result = 0;
            //v16
            var val2 = 1;

            //v28
            var index = 0;
            //v29
            var loaderID = -1;
            switch (npcID)
            {
                case NPCID.Baby:
                    {
                        //var childParameters = SV.childParameters
                        //if (BustupChildParameter.get_Gender(childParameters[0]) == 1)
                        var charID = 30;
                        //else
                        //charID = 31
                        //...
                        //val1 = BustupChildParameter.Character;
                        val1 = 1;
                        val2 = 1;

                        var buData = BustupDataList.Datas[charID];
                        var size = buData.Data.Count;

                        while (true)
                        {
                            var data = buData.Data[index];
                            if (data.Val1 != val1 || data.Val2 != val2)
                            {
                                result = loaderID;
                                size = buData.Data.Count;
                                //loaderID = result;
                                if (++index >= size)
                                    return result;
                            }
                            else
                            {
                                var poseNo = data.PoseNo;
                                result = data.LoadID;
                                if (poseNo == poseID)
                                    break;
                                size = buData.Data.Count;
                                loaderID = result;
                                if (++index >= size)
                                    return result;
                            }
                        }
                    }
                    break;
                default:
                    {
                        var charID = (int)CheckChangeBUSTUPID(npcID, poseID, costumeID);
                        val1 = 0;
                        val2 = 0;
                        var buData = BustupDataList.Datas[charID];
                        var size = buData.Data.Count;
                        
                        while (true)
                        {
                            var data = buData.Data[index];
                            if (data.Val1 != val1 || data.Val2 != val2)
                            {
                                result = loaderID;
                                size = buData.Data.Count;
                                //loaderID = result;
                                if (++index >= size)
                                    return result;
                            }
                            else
                            {
                                var poseNo = data.PoseNo;
                                result = data.LoadID;
                                if (poseNo == poseID)
                                    break;
                                size = buData.Data.Count;
                                loaderID = result;
                                if (++index >= size)
                                    return result;
                            }
                        }
                    }
                    break;
            }
            if (result < 0)
                return loaderID;
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
                            //var array = new BUSTUPID[] { BUSTUPID.Fuqua3, BUSTUPID.Fuqua2, BUSTUPID.Fuqua3 };
                            //return (costumeID - 2 >= 3) ? (BUSTUPID)5 : array[costumeID - 2];

                            //This wouldn't work with costumeID 1; only 2-4. This needs more research...
                            break;
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