using Combat.Component.Body;
using Combat.Component.Mods;
using Constructor.Model;
using UnityEngine;

namespace Combat.Component.Engine
{
    public class ShipEngine : IEngine
    {
        /// Legacy physics model constants
        private const float _extraAccelerationScale = 3.0f;
        private const float _extraAccelerationMax = 2.0f;

        public ShipEngine(
            EngineStats engineStats,
            EngineStats engineStatsWithoutEnergy)
        {
            _engineStats = engineStats;
            _engineStatsWithoutEnergy = engineStatsWithoutEnergy;
            UpdateData(true);
        }

        public float MaxVelocity => _engineData.Velocity;
        public float MaxAngularVelocity => _engineData.AngularVelocity;
        public float Propulsion => _engineData.Propulsion;
        public float TurnRate => _engineData.TurnRate;

        public float ForwardAcceleration { get; private set; }

        public float? Course
        {
            get => _engineData.HasCourse ? (float?)_engineData.Course : null;
            set
            {
                if (value.HasValue)
                {
                    _engineData.HasCourse = true;
                    _engineData.Course = value.Value;
                }
                else
                {
                    _engineData.HasCourse = false;
                }
            }
        }

        public float Throttle { get => _engineData.Throttle; set => _engineData.Throttle = value; }

        public Modifications<EngineData> Modifications => _modifications;

        public void Update(float elapsedTime, IBody body, bool hasEnergy)
        {
            if (!hasEnergy && _engineStatsWithoutEnergy.IsNull)
            {
                Throttle = 0;
                return;
            }

            UpdateData(hasEnergy);

            /// Update dynamic limits on physical body with headroom for turning inertia
            if (RigidBodyAdapter.UseDynamicPhysicsLimits && body is IBodyComponent bodyComponent)
            {
                bodyComponent.SetVelocityLimit(_engineStats.VelocityLimit);
                bodyComponent.SetAngularVelocityLimit(MaxAngularVelocity * 1.5f);
            }

            ForwardAcceleration = Throttle > 0.01f ? ApplyAcceleration(body, elapsedTime) : 0f;

            if (_engineData.Deceleration > 0)
                ApplyDeceleration(body, elapsedTime);

            if (_engineData.HasCourse)
                ApplyAngularAcceleration(body, elapsedTime);
            else if (Mathf.Abs(body.AngularVelocity) > 0.01f)
                ApplyAngularDeceleration(body, elapsedTime);
        }

        private void UpdateData(bool hasEnergy)
        {
            var stats = hasEnergy ? _engineStats : _engineStatsWithoutEnergy;

            _engineData.AngularVelocity = stats.AngularVelocity;
            _engineData.Velocity = stats.Velocity;
            _engineData.TurnRate = stats.TurnRate;
            _engineData.Propulsion = stats.Propulsion;
            _engineData.Deceleration = 0;

            _modifications.Apply(ref _engineData);

            if (RigidBodyAdapter.UseDynamicPhysicsLimits)
            {
                /// Legacy behavior: cap target velocity to nominal ship speed so afterburner focuses on thrust and agility
                if (_engineData.Velocity > stats.Velocity)
                    _engineData.Velocity = stats.Velocity;
                if (_engineData.AngularVelocity > stats.AngularVelocityLimit)
                    _engineData.AngularVelocity = stats.AngularVelocityLimit;
            }
            else
            {
                /// Default behavior: clamp velocity to global maximum ceiling
                if (_engineData.Velocity > stats.VelocityLimit)
                    _engineData.Velocity = stats.VelocityLimit;
                if (_engineData.AngularVelocity > stats.AngularVelocityLimit)
                    _engineData.AngularVelocity = stats.AngularVelocityLimit;
            }
        }

        private float ApplyAcceleration(IBody body, float elapsedTime)
        {
            if (RigidBodyAdapter.UseDynamicPhysicsLimits)
            {
                /// Legacy model: split axis thrust with natural inertia and takeoff boost
                var forward = RotationHelpers.Direction(body.Rotation);
                var side = new Vector2(forward.y, -forward.x);
                var velocity = body.Velocity;
                var forwardVelocity = Vector2.Dot(velocity, forward);
                var sideVelocity = Vector2.Dot(velocity, side);

                var extraAcceleration = Mathf.Min(Propulsion * _extraAccelerationScale, _extraAccelerationMax);
                var maxPropulsion = Propulsion + extraAcceleration;

                var forwardAcceleration = Throttle * CalculateOldAcceleration(
                    forwardVelocity,
                    MaxVelocity * 0.1f,
                    MaxVelocity,
                    _engineStats.VelocityLimit,
                    Propulsion,
                    extraAcceleration);

                var sideAcceleration = Mathf.Clamp(-sideVelocity, -maxPropulsion, maxPropulsion) * Throttle;

                if (forwardAcceleration < 0.01f && sideAcceleration < 0.01f && sideAcceleration > -0.01f)
                    return 0f;

                var sqrMagnitude = (forwardAcceleration * forwardAcceleration + sideAcceleration * sideAcceleration) / (maxPropulsion * maxPropulsion);
                if (sqrMagnitude > 1.0f)
                {
                    var magnitude = Mathf.Sqrt(sqrMagnitude);
                    forwardAcceleration /= magnitude;
                    sideAcceleration /= magnitude;
                }

                body.ApplyAcceleration(elapsedTime * forwardAcceleration * forward + elapsedTime * sideAcceleration * side);
                return forwardAcceleration;
            }
            else
            {
                /// Default model: directional drift compensation
                var forward = RotationHelpers.Direction(body.Rotation);
                var velocity = body.Velocity;
                var forwardVelocity = Vector2.Dot(velocity, forward);
                var requiredVelocity = CalculateRequiredVelocity(forwardVelocity, MaxVelocity);
                var propulsionVector = CalculatePropulsionVector(velocity, requiredVelocity * forward, MaxVelocity);
                var acceleration = Propulsion * Throttle * propulsionVector;

                body.ApplyAcceleration(elapsedTime * acceleration);
                return Vector2.Dot(acceleration, forward);
            }
        }

        /// Legacy non-linear acceleration curves
        private static float CalculateOldAcceleration(float velocity, float minVelocity, float targetVelocity, float maxVelocity, float maxAcceleration, float extraAcceleration)
        {
            if (velocity >= maxVelocity)
                return 0f;

            if (maxAcceleration < 0.01f)
                return 0f;

            if (velocity < 0)
                return 2 * maxAcceleration;

            if (velocity < minVelocity)
            {
                var scale = (minVelocity - velocity) / minVelocity;
                return maxAcceleration + extraAcceleration * scale * scale;
            }

            if (velocity <= targetVelocity)
                return maxAcceleration;

            if (velocity < maxVelocity - 0.01f)
            {
                var scale = 0.1f * (maxVelocity - velocity) / (maxVelocity - targetVelocity);
                return (maxAcceleration + extraAcceleration) * scale;
            }

            return 0f;
        }

        /// Default model velocity helper
        private static float CalculateRequiredVelocity(float velocity, float engineMaxVelocity)
        {
            if (velocity > engineMaxVelocity) return velocity;
            return engineMaxVelocity;
        }

        /// Default model drift compensation vector calculation
        private static Vector2 CalculatePropulsionVector(in Vector2 velocity, in Vector2 requiredVelocity, float maxSpeed)
        {
            var direction = requiredVelocity - velocity;
            var length = direction.magnitude;
            if (length < 0.001f) return Vector2.zero;
            return direction / Mathf.Max(0.5f * length, maxSpeed);
        }

        private void ApplyDeceleration(IBody body, float elapsedTime)
        {
            var velocity = body.Velocity;
            if (velocity.magnitude < 0.001f)
                return;

            var direction = velocity.normalized;
            body.ApplyAcceleration(-_engineData.Deceleration * elapsedTime * direction);
        }

        private void ApplyAngularAcceleration(IBody body, float elapsedTime)
        {
            var angularVelocity = body.AngularVelocity;
            var acceleration = 0f;

            var minDeltaAngle = Mathf.DeltaAngle(body.Rotation, _engineData.Course);

            var deltaAngle = 0f;
            if (minDeltaAngle > 0 && angularVelocity < 0)
                deltaAngle = 360 - minDeltaAngle;
            else if (minDeltaAngle < 0 && angularVelocity > 0)
                deltaAngle = 360 + minDeltaAngle;
            else
                deltaAngle = Mathf.Abs(minDeltaAngle);

            /// Angular acceleration rate with dynamic boost support
            var maxTurnRate = RigidBodyAdapter.UseDynamicPhysicsLimits
                ? TurnRate + Mathf.Min(_extraAccelerationScale * TurnRate, _extraAccelerationMax)
                : TurnRate;

            if (deltaAngle < 120f && deltaAngle < angularVelocity * angularVelocity / TurnRate)
                acceleration = Mathf.Clamp(-angularVelocity, -TurnRate * elapsedTime, TurnRate * elapsedTime);
            else if (minDeltaAngle < 0 && angularVelocity > -MaxAngularVelocity * 1.5f)
            {
                var min = angularVelocity > MaxAngularVelocity * 0.1f ? -maxTurnRate : angularVelocity > -MaxAngularVelocity ? -TurnRate : -maxTurnRate * 0.1f;
                acceleration = Mathf.Max(minDeltaAngle, min * elapsedTime);
            }
            else if (minDeltaAngle > 0 && angularVelocity < MaxAngularVelocity * 1.5f)
            {
                var max = angularVelocity < MaxAngularVelocity * 0.1f ? maxTurnRate : angularVelocity < MaxAngularVelocity ? TurnRate : maxTurnRate * 0.1f;
                acceleration = Mathf.Min(minDeltaAngle, max * elapsedTime);
            }
            else
                return;

            body.ApplyAngularAcceleration(acceleration);
        }

        private void ApplyAngularDeceleration(IBody body, float elapsedTime)
        {
            var acceleration = Mathf.Clamp(-body.AngularVelocity, -TurnRate * elapsedTime, TurnRate * elapsedTime);
            body.ApplyAngularAcceleration(acceleration);
        }

        private EngineData _engineData;
        private readonly EngineStats _engineStats;
        private readonly EngineStats _engineStatsWithoutEnergy;
        private readonly Modifications<EngineData> _modifications = new Modifications<EngineData>();
    }
}