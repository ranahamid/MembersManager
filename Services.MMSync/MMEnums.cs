using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MMSync
{
    public enum MMStatus : int
    {
        New,
        Updated,
        Removed,
        BadData,
        FromDb,
        UpdateAndSubscribe,
        EmailEmpty,
        EmailDuplicate,
        EmailInvalid
    }

    public enum MMEnums : int
    {
        ExternalId,
        Firstname,
        Lastname,
        Address,
        Address2,
        Postcode,
        City,
        Country,
        Phone,
        Mobile,
        Email,
        CVRnummer,
        BrugerID,
        Medlemsstatus,
        Foreningsnummer,
        Foedselsaar,
        HektarDrevet,
        AntalAmmekoeer,
        AntalAndetKvaeg,
        AntalSlagtekvaeg,
        AntaMalkekoeer,
        AntalSlagtesvin,
        AntalSoeer,
        AntalAarssoeer,
        AntalPelsdyr,
        AntalHoens,
        AntalKyllinger,
        Ecology,
        Sektion_SSJ,
        Driftform_planteavl,
        Driftform_Koed_Koer,
        Driftform_Mælk,
        Driftform_Svin,
        Driftform_Pelsdyr,
        Driftform_Aeg_Kylling,
        Driftstoerrelse_Planteavl,
        Driftstoerrelse_Koed_Koer,
        Driftfstoerrelse_Mælk,
        Driftstoerrelse_Svin,
        Driftstoerrelse_Pelsdyr,
        Driftstoerrelse_Aeg_Kylling
    }
}
