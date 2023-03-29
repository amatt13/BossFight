using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BossFight;
using BossFight.Extentions;
using BossFight.Models;
using Microsoft.Extensions.Logging;

public class PlayerRegenerator
{
    private ILogger<PlayerRegenerator> _logger;

    private readonly int minuteInMilliseconds = 60_000;
    public readonly int HP_INTERVAL = 6000;
    public readonly int MANA_INTERVAL = 6000;
    
    public PlayerRegenerator(ILogger<PlayerRegenerator> logger){ 
        _logger = logger;
        _logger.LogInformation("Started regen loop");
    }

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
            var durationIsSeconds  = durationInMilliseconds/1000;
            _logger.LogInformation("Regen :  We are behind schedule!! It took {durationIsSeconds} seconds to regen hp/mana...", durationIsSeconds);
        }
        else
            Thread.Sleep(minuteInMilliseconds - durationInMilliseconds);

        new Thread(RegenLoop).Start();
    }

    private void Regen()
    {
        var players = new Player().FindAll();
        using var connection = GlobalConnection.GetNewOpenConnection();

        var hpRegenCmd = @"UPDATE Player p 
SET p.Hp = p.Hp + @hpToRestore  
WHERE p.PlayerId = @playerId
AND p.Hp + @hpToRestore <= @maxHp";

            var manaRegenCmd = @"UPDATE Player p 
SET p.Mana = p.Mana + @manaToRestore
WHERE p.PlayerId = @playerId
AND p.Mana + @manaToRestore <= @maxMana";

        foreach(var player in players)
        {
            var maxHp = player.PlayerPlayerClass.MaxHp;
            var maxMana = player.PlayerPlayerClass.MaxMana;
            var hpToRestore = player.PlayerPlayerClass.PlayerClass.HpRegenRate;
            var manaToRestore = player.PlayerPlayerClass.PlayerClass.ManaRegenRate;
            var playerId = player.PlayerId;
            
            try
            {
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
            }
            catch (Exception ex)
            {
                // Log the error information and continue processing the remaining players
                _logger.LogError("Error regenerating player's hp and mana for {playerId}: {ex.Message}", playerId, ex.Message);
            }
        }
        connection.Close();
    }
}
