using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Gem.Common;

namespace Gem
{
    public class SpacialComponent : Component, Network.ISyncable
    {
        public static Action<SpacialComponent> OnBoundsChanged;

        private Vector3 _position;
        private Quaternion _orientation;
        private BoundingSphere _boundingVolume = new BoundingSphere(Vector3.Zero, 1.0f);

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _boundingVolume.Center = _position;
                if (OnBoundsChanged != null) OnBoundsChanged(this);
            }
        }

        public Quaternion Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public BoundingSphere BoundingVolume
        {
            get { return _boundingVolume; }
            set
            {
                _boundingVolume = value;
                _boundingVolume.Center = _position;
                if (OnBoundsChanged != null) OnBoundsChanged(this);
            }
        }

        uint Network.ISyncable.EntityID
        {
            get { return this.EntityID; }
        }

        uint Network.ISyncable.SyncID
        {
            get { return this.SyncID; }
        }

        void Network.ISyncable.writeFullSync(Network.WriteOnlyDatagram datagram)
        {
            datagram.WriteBytes(BitConverter.GetBytes(Position.X));
            datagram.WriteBytes(BitConverter.GetBytes(Position.Y));
            datagram.WriteBytes(BitConverter.GetBytes(Position.Z));
            datagram.WriteBytes(BitConverter.GetBytes(Orientation.X));
            datagram.WriteBytes(BitConverter.GetBytes(Orientation.Y));
            datagram.WriteBytes(BitConverter.GetBytes(Orientation.Z));
            datagram.WriteBytes(BitConverter.GetBytes(Orientation.W));
        }

        void Network.ISyncable.readFullSync(Network.ReadOnlyDatagram datagram)
        {
            var data = new byte[sizeof(Single) * 7];
            datagram.ReadBytes(data, sizeof(Single) * 7);
            Orientation = new Quaternion(BitConverter.ToSingle(data, sizeof(Single) * 3),
                BitConverter.ToSingle(data, sizeof(Single) * 4),
                BitConverter.ToSingle(data, sizeof(Single) * 5),
                BitConverter.ToSingle(data, sizeof(Single) * 6));
            Position = new Vector3(BitConverter.ToSingle(data, sizeof(Single) * 0),
                BitConverter.ToSingle(data, sizeof(Single) * 1),
                BitConverter.ToSingle(data, sizeof(Single) * 2));
        }
    }
}
