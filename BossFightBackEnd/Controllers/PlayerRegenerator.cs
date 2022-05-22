using System;
using System.Threading;
using System.Threading.Tasks;
using BossFight;
using BossFight.Extentions;
using BossFight.Models;

public class PlayerRegenerator
{
    private readonly int minuteInMilliseconds = 60_000;
    public readonly int HP_INTERVAL = 6000;
    public readonly int MANA_INTERVAL = 6000;
    public PlayerRegenerator(){ }
    public void Run()
    {
        new Thread(RegenLoop).Start();
    }

    private void RegenLoop()
    {
        var startTime = DateTime.Now;
        
        Regen();

        var currentTime = DateTime.Now;
        var durationInMilliseconds = (currentTime - startTime).Milliseconds;
        if (durationInMilliseconds > minuteInMilliseconds)
        {
            // should never happend...
            Console.WriteLine($"Regen :  We are behind schedule!! It took { durationInMilliseconds/1000 } seconds to regen hp/mana...");
        }
        else
            Thread.Sleep(minuteInMilliseconds - durationInMilliseconds);

        new Thread(RegenLoop).Start();
    }

    private void Regen()
    {
        var players = new Player().FindAll();
        foreach(var player in players)
        {
            var maxHp = player.PlayerPlayerClass.MaxHp;
            var maxMana = player.PlayerPlayerClass.MaxMana;
            var hpToRestore = player.PlayerPlayerClass.PlayerClass.HpRegenRate;
            var manaToRestore = player.PlayerPlayerClass.PlayerClass.ManaRegenRate;
            var playerId = player.PlayerId;

            var hpRegenCmd = @"UPDATE Player p 
SET p.Hp = p.Hp + @hpToRestore  
WHERE p.PlayerId = @playerId
AND p.Hp + @hpToRestore <= @maxHp";

            var manaRegenCmd = @"UPDATE Player p 
SET p.Mana = p.Mana + @manaToRestore
WHERE p.PlayerId = @playerId
AND p.Mana + @manaToRestore <= @maxMana";

            using var connection = GlobalConnection.GetNewOpenConnection();
            
            using var hpCmd = connection.CreateCommand();
            hpCmd.CommandText = hpRegenCmd;
            hpCmd.Parameters.AddParameter(hpToRestore.ToDbString(), "@hpToRestore");
            hpCmd.Parameters.AddParameter(maxHp.ToDbString(), "@maxHp");
            hpCmd.Parameters.AddParameter(playerId.ToDbString(), "@playerId");
            hpCmd.ExecuteNonQuery();

            using var manaCmd = connection.CreateCommand();
            manaCmd.CommandText = manaRegenCmd;
            manaCmd.Parameters.AddParameter(manaToRestore.ToDbString(), "@manaToRestore");
            manaCmd.Parameters.AddParameter(maxMana.ToDbString(), "@maxMana");
            manaCmd.Parameters.AddParameter(playerId.ToDbString(), "@playerId");
            manaCmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}
