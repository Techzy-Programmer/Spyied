using System;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace Controller.Codes
{
    public static class FutureUI
    {
        #region Declaration

        private static long TotalFutID = 0;
        private static readonly Random HGen = new Random();
        private static readonly Dictionary<long, KeyValuePair<object, Dictionary<string, object>[]>>
            FutureDictionary = new Dictionary<long, KeyValuePair<object, Dictionary<string, object>[]>>();

        #endregion

        #region Main Methods

        public static long SetFuture(this object FutObj, object FutProps, string CText = null, bool IsDisable = true)
        {
        ReGENId:;
            long FutID = TotalFutID++;
            
            if (FutureDictionary.ContainsKey(FutID)) goto ReGENId;
            else
            {
                var RollD = new Dictionary<string, object>();
                var FutD = GetExpando(FutProps).ToDictionary(X => X.Key, X => X.Value);
                foreach (string Prop in FutD.Keys) RollD.Add(Prop, FutObj.GetProp(Prop));
                FutureDictionary.Add(FutID, new KeyValuePair<object, Dictionary<string, object>[]>
                    (FutObj, new Dictionary<string, object>[] { FutD, RollD }));
            }

            if (FutObj is Control UCtrl)
            {
                UCtrl.Enabled = !IsDisable;
                if (!string.IsNullOrWhiteSpace(CText)) UCtrl.Text = CText;
            }

            return FutID;
        }

        public static void UpdateFuture(long FutID, bool IsSucess)
        {
            if (FutureDictionary.ContainsKey(FutID))
            {
                var FutDets = FutureDictionary[FutID];
                var RollDict = FutDets.Value[1];
                var FutDict = FutDets.Value[0];

                if (IsSucess)
                {
                    foreach (var KVP in FutDict.ToArray())
                    {
                        if (FutDets.Key is Control UCtrl) UCtrl.SInvoke((C) =>
                        { C.Enabled = true; C.SetProp(KVP.Key, KVP.Value); });
                        else FutDets.Key.SetProp(KVP.Key, KVP.Value);
                    }
                }
                else
                {
                    foreach (var KVP in RollDict.ToArray())
                    {
                        if (FutDets.Key is Control UCtrl) UCtrl.SInvoke((C) =>
                        { C.Enabled = true; C.SetProp(KVP.Key, KVP.Value); });
                        else FutDets.Key.SetProp(KVP.Key, KVP.Value);
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        public static void SetProp(this object PObj, string PropName, object PropValue)
        {
            PropertyInfo PropInfo = PObj.GetType().GetProperty(PropName);
            if (PropInfo != null && PropInfo.CanWrite) PropInfo.SetValue(PObj, PropValue, null);
        }

        public static object GetProp(this object PObj, string PropName)
        {
            PropertyInfo PropInfo = PObj.GetType().GetProperty(PropName);
            if (PropInfo != null && PropInfo.CanRead) return PropInfo.GetValue(PObj);
            else return null;
        }

        private static ExpandoObject GetExpando(object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        #endregion
    }
}


//private static string GetFutureHash
//{
//    get
//    {
//        return $"Hash>" +
//            HGen.Next(100000000, 999999999).ToString() +
//            (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()).ToString();
//    }
//}