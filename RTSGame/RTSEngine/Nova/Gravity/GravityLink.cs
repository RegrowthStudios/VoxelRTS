using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NovaLibrary.Object;

namespace NovaLibrary.Gravity
{
    public class GravityLink
    {
        public const float G_Constant = 0.14f;

        IGravityDonor donor;
        IGravityRecipient recipient;

        public GravityLink(IGravityDonor donor, IGravityRecipient recipient)
        {
            this.donor = donor;
            this.recipient = recipient;
        }

        public bool hasObject(object o)
        {
            return o == donor || o == recipient;
        }

        public void update(float dTime)
        {
            recipient.accelerateG(donor.gAcceleration(recipient), dTime);
        }

        public static bool willLink(NOVA_TYPE don, NOVA_TYPE rec)
        {
            switch (don)
            {
                case NOVA_TYPE.TETHER:
                    switch (rec)
                    {
                        case NOVA_TYPE.STAR:
                        case NOVA_TYPE.STAR_DUST:
                            return true;
                        default:
                            return false;
                    }
                case NOVA_TYPE.SINK_HOLE:
                    switch (rec)
                    {
                        case NOVA_TYPE.STAR_DUST:
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }
    }
}
