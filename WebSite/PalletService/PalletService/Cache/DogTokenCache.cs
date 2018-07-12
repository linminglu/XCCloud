using PalletService.Model.WorkStation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalletService.Cache
{
    public class DogTokenCache
    {
        private static Hashtable ht = new Hashtable();

        public static void AddToken(DogTokenModel dogTokenModel)
        {
            var query = from item in ht.Cast<DictionaryEntry>() 
                        where ((DogTokenModel)(item.Value)).DogId.Equals(dogTokenModel.DogId) && 
                              ((DogTokenModel)(item.Value)).WorkStation.Equals(dogTokenModel.WorkStation)
                        select item.Key.ToString();
            if (query.Count() > 0)
            {
                string token = query.First();
                ht.Remove(token);
            }
            ht[dogTokenModel.Token] = dogTokenModel;
        }

        public static bool GetToken(string dogId, string workStation, out string token, ref DogTokenModel dogTokenModel)
        {
            token = string.Empty;
            var query = from item in ht.Cast<DictionaryEntry>()
                        where ((DogTokenModel)(item.Value)).DogId.Equals(dogId) &&
                              ((DogTokenModel)(item.Value)).WorkStation.Equals(workStation)
                        select item.Key.ToString();
            if (query.Count() > 0)
            {
                token = query.First();
                dogTokenModel = (DogTokenModel)(ht[dogTokenModel.Token]);
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}
