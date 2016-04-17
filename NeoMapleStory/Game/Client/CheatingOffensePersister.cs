﻿using System.Collections.Generic;
using NeoMapleStory.Game.Client.AntiCheat;

namespace NeoMapleStory.Game.Client
{
    public class CheatingOffensePersister
    {
        public static CheatingOffensePersister Instance { get; } = new CheatingOffensePersister();
        private List<CheatingOffenseEntry> _toPersist = new List<CheatingOffenseEntry>();

        private CheatingOffensePersister()
        {
            //TimerManager.Instance.RegisterJob(new PersistingTask(), 61000);
        }

        public void PersistEntry(CheatingOffenseEntry coe)
        {
            lock (_toPersist)
            {
                _toPersist.Remove(coe); //equal/hashCode h4x
                _toPersist.Add(coe);
            }
        }

        // public class PersistingTask
        //{


        //    CheatingOffenseEntry[] offenses;
        //    synchronized(toPersist)
        //    {
        //        offenses = toPersist.toArray(new CheatingOffenseEntry[toPersist.size()]);
        //        toPersist.clear();
        //    }

        //    Connection con = DatabaseConnection.getConnection();
        //    try
        //    {
        //        PreparedStatement insertps = con.prepareStatement("INSERT INTO cheatlog (cid, offense, count, lastoffensetime, param) VALUES (?, ?, ?, ?, ?)");
        //    PreparedStatement updateps = con.prepareStatement("UPDATE cheatlog SET count = ?, lastoffensetime = ?, param = ? WHERE id = ?");
        //        for (CheatingOffenseEntry offense : offenses)
        //        {
        //            String parm = offense.getParam() == null ? "" : offense.getParam();
        //            if (offense.getDbId() == -1)
        //            {
        //                insertps.setInt(1, offense.getChrfor().getId());
        //                insertps.setString(2, offense.getOffense().name());
        //                insertps.setInt(3, offense.getCount());
        //                insertps.setTimestamp(4, new Timestamp(offense.getLastOffenseTime()));
        //                insertps.setString(5, parm);
        //                insertps.executeUpdate();
        //                ResultSet rs = insertps.getGeneratedKeys();
        //                if (rs.next())
        //                {
        //                    offense.setDbId(rs.getInt(1));
        //                }
        //rs.close();
        //            }
        //            else {
        //                updateps.setInt(1, offense.getCount());
        //                updateps.setTimestamp(2, new Timestamp(offense.getLastOffenseTime()));
        //                updateps.setString(3, parm);
        //                updateps.setInt(4, offense.getDbId());
        //                updateps.executeUpdate();
        //            }
        //        }
        //        insertps.close();
        //        updateps.close();
        //    }
        //    catch (SQLException e)
        //    {
        //        log.error("error persisting cheatlog", e);
        //    }
        //}

    }
}
