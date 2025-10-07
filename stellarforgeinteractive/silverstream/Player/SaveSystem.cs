using System;
using System.Linq;
using UnityEngine.Windows;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public class SaveSystem
    {
        public static void Save(PlayerStatController psc, CurrencyTracker currencyTracker)
        {
            try
            {
                Directory.CreateDirectory("./data");
                File.WriteAllBytes("./data/fish", BitConverter.GetBytes(currencyTracker.Currency));
                var list = Array.Empty<byte>().ToList();
                list.AddRange(BitConverter.GetBytes(psc.StaminaLevel));
                list.AddRange(BitConverter.GetBytes(psc.DashUnlocked));
                list.AddRange(BitConverter.GetBytes(psc.WallJumpUnlocked));
                File.WriteAllBytes("./data/power", list.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}