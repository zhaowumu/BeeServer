using System;


namespace BeeGame.Protocol.Message
{
    [Serializable]
    public class LoginQ : BeeMessage
    {
        public string Name { get; set; }

        public string Password { get; set; }
    }
}
