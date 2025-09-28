namespace cowsins
{
    using UnityEngine;
    using UnityEngine.Events;

    public class WeaponDefaultState : WeaponBaseState
    {
        private WeaponController controller;

        private PlayerStats stats;

        private InteractManager interact;

        private PlayerMovement movement;

        private float holdProgress;

        private float noBulletIndicator;

        private bool holdingEmpty = false;

        private UnityEvent shootMethodAction;

        private Weapon_SO backUpWeapon;

        public WeaponDefaultState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            controller = _ctx.GetComponent<WeaponController>();
            stats = _ctx.GetComponent<PlayerStats>();
            interact = _ctx.GetComponent<InteractManager>();
            movement = _ctx.GetComponent<PlayerMovement>();

            holdProgress = 0;

            holdingEmpty = false;

            InputManager.onInspect += SwitchToInspect;
            InputManager.onMelee += SwitchToMelee;
            InputManager.onToggleFlashlight += controller.ToggleFlashLight;
            shootMethodAction = new UnityEvent();
            controller.events.OnUnholsterWeapon.AddListener(UpdateWeaponShootEvents);
            UpdateWeaponShootEvents();
        }

        private void UpdateWeaponShootEvents()
        {
            if (controller.weapon == backUpWeapon) return;
            
            backUpWeapon = controller.weapon;
            
            switch (controller.weapon?.shootMethod)
            {
                case ShootingMethod.Press: shootMethodAction.AddListener(PressShoot); break;
                case ShootingMethod.PressAndHold: shootMethodAction.AddListener(PressHoldShoot); break;
                case ShootingMethod.HoldAndRelease: shootMethodAction.AddListener(HoldAndReleaseShoot); break;
                case ShootingMethod.HoldUntilReady: shootMethodAction.AddListener(HoldUntilReadyShoot); break;
            }
        }

        public override void UpdateState()
        {
            if (!stats.controllable) return;
            HandleInventory(); 
            if (controller.weapon == null || movement.Climbing) return;
            CheckAim();
        }

        public override void FixedUpdateState() {
            if (!stats.controllable || controller.weapon == null || movement.Climbing) return;
            CheckSwitchState();
        }

        public override void ExitState() 
        {
            InputManager.onInspect -= SwitchToInspect;
            InputManager.onMelee -= SwitchToMelee;
            InputManager.onToggleFlashlight -= controller.ToggleFlashLight;
            controller.events.OnEquipWeapon.RemoveListener(UpdateWeaponShootEvents);
        }
        public override void CheckSwitchState()
        {
            if (CanSwitchToShoot()) shootMethodAction?.Invoke();

            if (controller.weapon.audioSFX.emptyMagShoot != null)
            {
                if (controller.id.bulletsLeftInMagazine <= 0 && InputManager.shooting && noBulletIndicator <= 0 && !holdingEmpty)
                {
                    SoundManager.Instance.PlaySound(controller.weapon.audioSFX.emptyMagShoot, 0, .15f, true, 0);
                    noBulletIndicator = (controller.weapon.shootMethod == ShootingMethod.HoldAndRelease || controller.weapon.shootMethod == ShootingMethod.HoldUntilReady) ? 1 : controller.weapon.fireRate;
                    holdingEmpty = true;
                }

                if (noBulletIndicator > 0) noBulletIndicator -= Time.deltaTime;

                if (!InputManager.shooting) holdingEmpty = false;
            }

            if (controller.weapon.infiniteBullets) return;

            if (controller.weapon.reloadStyle == ReloadingStyle.defaultReload && !controller.shooting)
            {
                if (CanSwitchToReload(controller))
                    SwitchState(_factory.Reload());
            }
            else
            {
                if (controller.id.heatRatio >= 1) SwitchState(_factory.Reload());
            }

        }

        private void CheckAim()
        {
            if (InputManager.aiming && controller.weapon.allowAim) controller.Aim();
            CheckStopAim();
        }

        private void CheckStopAim() { if (!InputManager.aiming) controller.StopAim(); }

        private void HandleInventory() => controller.HandleInventory();

        private bool CanSwitchToReload(WeaponController controller)
        {
            return InputManager.reloading && (int)controller.weapon.shootStyle != 2 && controller.id.bulletsLeftInMagazine < controller.id.magazineSize && controller.id.totalBullets > 0
                        || controller.id.bulletsLeftInMagazine <= 0 && controller.autoReload && (int)controller.weapon.shootStyle != 2 && controller.id.bulletsLeftInMagazine < controller.id.magazineSize && controller.id.totalBullets > 0;
        }

        private void SwitchToInspect()
        {
            if (stats.controllable && controller.weapon && _ctx.inspectionUI.alpha <= 0 && interact.canInspect) SwitchState(_factory.Inspect());
        }

        private void SwitchToMelee()
        {
            if (stats.controllable && controller.CanMelee && controller.isMeleeReady && !movement.Climbing) SwitchState(_factory.Melee());
        }

        private bool CanSwitchToShoot()
        {
            return controller.CanShoot &&
                (controller.id.bulletsLeftInMagazine > 0 || controller.weapon.shootStyle == ShootStyle.Melee) // Melee weapons dont use bullets 
                && !controller.selectingWeapon
                && (movement.canShootWhileDashing && movement.dashing || !movement.dashing);
        }

        private void PressShoot()
        {
            if (InputManager.shooting && !controller.holding)
            {
                controller.holding = true; // We are holding 
                SwitchState(_factory.Shoot());
            }
        }
        private void PressHoldShoot()
        {
            if (InputManager.shooting) SwitchState(_factory.Shoot());
        }
        private void HoldAndReleaseShoot()
        {
            if (!InputManager.shooting)
            {
                if (holdProgress > 100) SwitchState(_factory.Shoot());
                holdProgress = 0;
            }
            if (InputManager.shooting)
            {
                holdProgress += Time.deltaTime * controller.weapon.holdProgressSpeed;
                controller.holding = true;
            }
        }
        private void HoldUntilReadyShoot()
        {
            if (!InputManager.shooting) holdProgress = 0;
            if (InputManager.shooting)
            {
                holdProgress += Time.deltaTime * controller.weapon.holdProgressSpeed;
                if (holdProgress > 100) SwitchState(_factory.Shoot());
            }
        }
    }
}