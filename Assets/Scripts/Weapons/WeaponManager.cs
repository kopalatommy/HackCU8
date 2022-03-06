using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnnaturalSelection.Audio;
using UnnaturalSelection.Character;

namespace UnnaturalSelection.Weapons
{
    public enum WeaponState
    {
        Idle,
        Firing,
        Reloading,
        MeleeAttacking,
        Interacting
    }

    [DisallowMultipleComponent]
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Defines the reference to the First Person Character Controller script.")]
        private MovementController fPController;

        [SerializeField]
        [Tooltip("Defines the reference to the character’s Main Camera transform.")]
        private Transform cameraTransformReference;

        [SerializeField]
        [Tooltip("Defines how far the character can search for interactive objects.")]
        private float interactionRadius = 2;

        [SerializeField]
        [Tooltip("Determines the GameObject tag identifier to ammo pickups.")]
        private string ammoTag = "Ammo";

        [SerializeField]
        [Tooltip("Determines the GameObject tag identifier to adrenaline packs pickups.")]
        private string adrenalinePackTag = "Adrenaline Pack";

        [SerializeField]
        [Tooltip("Sound played when the character pick up an Item or Weapon.")]
        private AudioClip itemPickupSound;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the volume of Item Pickup Sound played when the character pick up an Item or Weapon.")]
        private float itemPickupVolume = 0.3f;

        [SerializeField]
        [Tooltip("When activated, the character will change their current weapon for others instantly.")]
        private bool fastChangeWeapons;

        [SerializeField]
        private List<Gun> equippedWeaponsList = new List<Gun>();

        [SerializeField]
        private List<Gun> weaponList = new List<Gun>();

        [SerializeField]
        private List<AmmoInstance> ammoList = new List<AmmoInstance>();

        [SerializeField]
        [Tooltip("The Default Weapon is equipped if the Equipped Weapons list is empty.")]
        private MeleeWeapon defaultWeapon;

        [SerializeField]
        [Tooltip("Defines the reference to the Frag Grenade item.")]
        private Grenade fragGrenade;

        [SerializeField]
        [Tooltip("Defines the reference to the Adrenaline item.")]
        private FirstAidKit adrenaline;

        private bool itemCoolDown;
        private bool isClimbing;
        private bool onLadder;

        private Camera _camera;
        private IWeapon currentWeapon;
        private AudioEmitter playerBodySource;

        private InputActionMap weaponMap;
        private InputActionMap movementMap;
        private InputAction nextWeaponAction;
        private InputAction previousWeaponAction;

        private InputAction lethalEquipmentAction;
        private InputAction tacticalEquipmentAction;

        private InputAction interactAction;

        public WeaponState State
        {
            get
            {
                switch (currentWeapon)
                {
                    case null:
                        return WeaponState.Idle;
                    case Gun gun when gun.Idle:
                        return WeaponState.Idle;
                    case Gun gun when gun.Firing:
                        return WeaponState.Firing;
                    case Gun gun when gun.Reloading:
                        return WeaponState.Reloading;
                    case Gun gun when gun.MeleeAttacking:
                        return WeaponState.MeleeAttacking;
                    case Gun gun when gun.Interacting:
                        return WeaponState.Interacting;
                    case MeleeWeapon meleeWeapon when meleeWeapon.Idle:
                        return WeaponState.Idle;
                    case MeleeWeapon meleeWeapon when meleeWeapon.MeleeAttacking:
                        return WeaponState.MeleeAttacking;
                    case MeleeWeapon meleeWeapon when meleeWeapon.Interacting:
                        return WeaponState.Interacting;
                    default:
                        return WeaponState.Idle;
                }
            }
        }

        public System.Type Type => currentWeapon?.GetType();

        public float? Accuracy
        {
            get
            {
                if (currentWeapon != null && currentWeapon.GetType() == typeof(Gun))
                    return ((Gun)currentWeapon).Accuracy;
                return null;
            }
        }

        public int CurrentAmmo
        {
            get
            {
                if (currentWeapon != null && currentWeapon.GetType() == typeof(Gun))
                    return ((Gun)currentWeapon).CurrentRounds;
                return -1;
            }
        }

        public int Magazines
        {
            get
            {
                if (currentWeapon is Gun gun)
                {
                    return GetAmmoInstance(gun.GunData.AmmoType).Amount;
                }
                return -1;
            }
        }

        public int GunID
        {
            get
            {
                if (currentWeapon != null)
                    return currentWeapon.Identifier;
                return -1;
            }
        }

        public string GunName
        {
            get
            {
                if (currentWeapon != null && currentWeapon.GetType() == typeof(Gun))
                {
                    return ((Gun)currentWeapon).GunName;
                }
                return string.Empty;
            }
        }
        public string FireMode
        {
            get
            {
                if (currentWeapon != null && currentWeapon.GetType() == typeof(Gun))
                    return (((Gun)currentWeapon).HasSecondaryMode ? ((Gun)currentWeapon).FireMode.ToString() : string.Empty);
                return string.Empty;
            }
        }

        public bool CanSwitch
        {
            get
            {
                if (currentWeapon == null)
                {
                    return true;
                }
                return currentWeapon != null && currentWeapon.CanSwitch;
            }
        }

        public Gun[] EquippedWeapons => equippedWeaponsList.ToArray();

        public Gun CurrentGun
        {
            get
            {
                if (currentWeapon != null && currentWeapon.GetType() == typeof(Gun))
                {
                    return (Gun)currentWeapon;
                }

                return null;
            }
        }

        public FirstAidKit Adrenaline => adrenaline;

        public Grenade FragGrenade => fragGrenade;

        public GameObject Target
        {
            get;
            private set;
        }

        public string AmmoTag => ammoTag;

        public string AdrenalinePackTag => adrenalinePackTag;

        public bool IsShotgun
        {
            get
            {
                if (currentWeapon != null && currentWeapon.GetType() == typeof(Gun))
                    return ((Gun)currentWeapon).FireMode == GunData.FireMode.ShotgunAuto || ((Gun)currentWeapon).FireMode == GunData.FireMode.ShotgunSingle;
                return false;
            }
        }
        public bool HasFreeSlot
        {
            get
            {
                for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
                {
                    if (!equippedWeaponsList[i])
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void AddWeaponSlot()
        {
            equippedWeaponsList.Add(null);
        }

        public void RemoveWeaponSlot(int index)
        {
            equippedWeaponsList.RemoveAt(index);
        }

        public void AddWeapon()
        {
            weaponList.Add(null);
        }

        public void RemoveWeapon(int index)
        {
            weaponList.RemoveAt(index);
        }

        private void Start()
        {
            _camera = cameraTransformReference.GetComponent<Camera>();

            // Disable all weapons
            if (defaultWeapon != null)
                defaultWeapon.Viewmodel.SetActive(false);

            for (int i = 0, c = weaponList.Count; i < c; i++)
            {
                if (weaponList[i] != null)
                    weaponList[i].Viewmodel.SetActive(false);
            }

            if (fragGrenade != null)
                fragGrenade.gameObject.SetActive(false);

            if (adrenaline != null)
                adrenaline.gameObject.SetActive(false);

            if (equippedWeaponsList.Count > 0)
            {
                for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
                {
                    if (equippedWeaponsList[i] == null)
                        continue;

                    // Initialize the weapon ammo
                    equippedWeaponsList[i].InitializeMagazineAsDefault();

                    if (currentWeapon == null)
                    {
                        Select(equippedWeaponsList[i], equippedWeaponsList[i].CurrentRounds);
                    }
                }
            }

            if (currentWeapon == null && defaultWeapon != null)
            {
                Select(defaultWeapon, 0);
            }

            CalculateWeight();

            // Input Bindings
            weaponMap = GameplayManager.Instance.GetActionMap("Weapons");
            weaponMap.Enable();

            nextWeaponAction = weaponMap.FindAction("Next Weapon");
            previousWeaponAction = weaponMap.FindAction("Previous Weapon");
            lethalEquipmentAction = weaponMap.FindAction("Lethal Equipment");
            tacticalEquipmentAction = weaponMap.FindAction("Tactical Equipment");

            movementMap = GameplayManager.Instance.GetActionMap("Movement");
            movementMap.Enable();

            interactAction = movementMap.FindAction("Interact");

            InvokeRepeating(nameof(Search), 0, 0.1f);
            playerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);

            fPController.ladderEvent += ClimbingLadder;
        }

        /// <summary>
        /// Notifies the equipped weapon that the character is climbing a ladder.
        /// </summary>
        /// <param name="climbing">Is the character climbing?</param>
        private void ClimbingLadder(bool climbing)
        {
            if (isClimbing == climbing)
                return;

            isClimbing = climbing;
            if (onLadder)
            {
                OnExitLadder();
            }
        }

        /// <summary>
        /// Deselect the current weapon to simulate climbing a ladder.
        /// </summary>
        private void OnEnterLadder()
        {
            onLadder = true;
            itemCoolDown = true;
            currentWeapon.Deselect();
        }

        /// <summary>
        /// Select the previous weapon and reactive all weapon features.
        /// </summary>
        private void OnExitLadder()
        {
            currentWeapon.Select();
            itemCoolDown = false;
            onLadder = false;
        }

        private void SelectByPreviousAndNextButtons()
        {
            int weaponIndex = GetEquippedWeaponIndexOnList(currentWeapon.Identifier);

            if (equippedWeaponsList.Count > 1)
            {
                if (nextWeaponAction.triggered)
                {
                    int newIndex = ++weaponIndex % equippedWeaponsList.Count;
                    if (equippedWeaponsList[newIndex])
                        StartCoroutine(Switch(currentWeapon, equippedWeaponsList[newIndex], equippedWeaponsList[newIndex].CurrentRounds));
                }
                else if (previousWeaponAction.triggered)
                {
                    int newIndex = --weaponIndex < 0 ? equippedWeaponsList.Count - 1 : weaponIndex;
                    if (equippedWeaponsList[newIndex])
                        StartCoroutine(Switch(currentWeapon, equippedWeaponsList[newIndex], equippedWeaponsList[newIndex].CurrentRounds));
                }
            }
        }

        private void Update()
        {
            // Analyze the character's target
            SearchForWeapons();
            SearchForAmmo();
            SearchForAdrenaline();
            SearchInteractiveObjects();

            if (!fPController.IsControllable)
                return;

            // Switch equipped weapons
            if (currentWeapon != null)
            {
                if (currentWeapon.CanSwitch)
                {
                    // If the character is climbing a ladder
                    if (isClimbing && !onLadder)
                    {
                        OnEnterLadder();
                    }
                    else
                    {
                        SelectByPreviousAndNextButtons();
                    }
                }
            }
            else
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, GameplayManager.Instance.FieldOfView, Time.deltaTime * 10);
            }

            // Throw a grenade
            if (!itemCoolDown)
            {
                if (lethalEquipmentAction.triggered && currentWeapon != null && currentWeapon.CanUseEquipment && fragGrenade && fragGrenade.Amount > 0)
                {
                    StartCoroutine(ThrowGrenade());
                }
            }

            // Use adrenaline
            if (!itemCoolDown)
            {
                if (tacticalEquipmentAction.triggered && currentWeapon != null && currentWeapon.CanUseEquipment && adrenaline && adrenaline.Amount > 0)
                {
                    StartCoroutine(AdrenalineShot());
                }
            }
        }

        private IEnumerator ThrowGrenade()
        {
            itemCoolDown = true;

            currentWeapon.Deselect();
            yield return new WaitForSeconds(currentWeapon.HideAnimationLength);
            currentWeapon.Viewmodel.SetActive(false);

            fragGrenade.gameObject.SetActive(true);
            fragGrenade.Init();
            fragGrenade.Use();

            yield return new WaitForSeconds(fragGrenade.UsageDuration);
            fragGrenade.gameObject.SetActive(false);

            currentWeapon.Viewmodel.SetActive(true);
            currentWeapon.Select();
            itemCoolDown = false;
        }

        private IEnumerator AdrenalineShot()
        {
            itemCoolDown = true;

            currentWeapon.Deselect();
            yield return new WaitForSeconds(currentWeapon.HideAnimationLength);
            currentWeapon.Viewmodel.SetActive(false);

            adrenaline.gameObject.SetActive(true);
            adrenaline.Init();
            adrenaline.Use();

            yield return new WaitForSeconds(adrenaline.UsageDuration);
            adrenaline.gameObject.SetActive(false);

            currentWeapon.Viewmodel.SetActive(true);
            currentWeapon.Select();
            itemCoolDown = false;
        }

        /// <summary>
        /// Checks the target object to analyze if it is a weapon.
        /// </summary>
        private void SearchForWeapons()
        {
            if (Target)
            {
                // Try to convert the target for a gun pickup.
                GunPickup target = Target.GetComponent<GunPickup>();

                // If the gun pickup is not null means that the target is actually a weapon.
                if (target)
                {
                    IWeapon weapon = GetWeaponByID(target.Identifier);

                    if (weapon == null)
                        return;

                    if (currentWeapon != null)
                    {
                        if (!currentWeapon.CanSwitch)
                            return;

                        if (IsEquipped(weapon))
                            return;

                        if (HasFreeSlot)
                        {
                            if (interactAction.triggered)
                            {
                                EquipWeapon(GetWeaponIndexOnList(weapon.Identifier));
                                Destroy(target.transform.gameObject);
                                StartCoroutine(Change(currentWeapon, weapon, target.CurrentRounds));

                                playerBodySource.ForcePlay(itemPickupSound, itemPickupVolume);
                                CalculateWeight();
                            }
                        }
                        else
                        {
                            if (interactAction.triggered)
                            {
                                UnequipWeapon(GetEquippedWeaponIndexOnList(currentWeapon.Identifier));
                                EquipWeapon(GetWeaponIndexOnList(weapon.Identifier));
                                StartCoroutine(DropAndChange(currentWeapon, weapon, target, target.CurrentRounds));

                                if (fastChangeWeapons)
                                    playerBodySource.ForcePlay(itemPickupSound, itemPickupVolume);

                                CalculateWeight();
                            }
                        }
                    }
                    else
                    {
                        if (HasFreeSlot)
                        {
                            if (interactAction.triggered)
                            {
                                EquipWeapon(GetWeaponIndexOnList(weapon.Identifier));
                                Select(weapon, target.CurrentRounds);
                                Destroy(target.transform.gameObject);
                                CalculateWeight();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the target object to analyze if it is a ammo box.
        /// </summary>
        private void SearchForAmmo()
        {
            if (!itemCoolDown && equippedWeaponsList.Count > 0 && currentWeapon != null
                && currentWeapon.CanUseEquipment && CanRefillAmmo())
            {
                if (Target)
                {
                    // If the target has the Ammo Tag
                    if (Target.CompareTag(ammoTag))
                    {
                        if (interactAction.triggered)
                        {
                            StartCoroutine(RefillAmmo());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the target object to analyze if it is a adrenaline pack.
        /// </summary>
        private void SearchForAdrenaline()
        {
            if (!itemCoolDown && equippedWeaponsList.Count > 0 && currentWeapon != null && currentWeapon.CanUseEquipment)
            {
                if (Target)
                {
                    // If the target has the Adrenaline Tag
                    if (Target.CompareTag(adrenalinePackTag) && adrenaline.CanRefill)
                    {
                        if (interactAction.triggered)
                        {
                            StartCoroutine(RefillItem(new Equipment[] { adrenaline }));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the target object to analyze if it is a interactive object.
        /// </summary>
        private void SearchInteractiveObjects()
        {
            if (!itemCoolDown)
            {
                if (Target)
                {
                    IActionable target = Target.GetComponent<IActionable>();

                    if (target != null)
                    {
                        if (interactAction.triggered)
                        {
                            StartCoroutine(Interact(target));
                        }
                    }
                }
            }
        }

        private IEnumerator RefillItem(Equipment[] items)
        {
            itemCoolDown = true;

            currentWeapon.Interact();
            yield return new WaitForSeconds(currentWeapon.InteractDelay);

            for (int i = 0; i < items.Length; i++)
            {
                items[i].Refill();
            }

            playerBodySource.ForcePlay(itemPickupSound, itemPickupVolume);

            yield return new WaitForSeconds(Mathf.Max(currentWeapon.InteractAnimationLength - currentWeapon.InteractDelay, 0));
            itemCoolDown = false;
        }

        public bool CanRefillAmmo()
        {
            for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
            {
                if (!equippedWeaponsList[i])
                    continue;

                AmmoInstance ammoInstance = GetAmmoInstance(equippedWeaponsList[i].GunData.AmmoType);
                if (ammoInstance.Amount != ammoInstance.MaxAmount)
                    return true;
            }

            if (fragGrenade.CanRefill)
                return true;

            return false;
        }

        /// <summary>
        /// Refills the character magazines for all equipped weapons.
        /// </summary>
        private IEnumerator RefillAmmo()
        {
            itemCoolDown = true;

            currentWeapon.Interact();
            yield return new WaitForSeconds(currentWeapon.InteractDelay);

            for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
            {
                if (!equippedWeaponsList[i])
                    continue;

                AmmoInstance ammoInstance = GetAmmoInstance(equippedWeaponsList[i].GunData.AmmoType);
                ammoInstance.Amount = ammoInstance.MaxAmount;
            }

            playerBodySource.ForcePlay(itemPickupSound, itemPickupVolume);

            // Also refill the grenades
            if (fragGrenade)
                fragGrenade.Refill();

            yield return new WaitForSeconds(Mathf.Max(currentWeapon.InteractAnimationLength - currentWeapon.InteractDelay, 0));
            itemCoolDown = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public int GetAmmo(AmmoType ammoType)
        {
            return GetAmmoInstance(ammoType)?.Amount ?? 0;
        }

        private AmmoInstance GetAmmoInstance(AmmoType ammoType)
        {
            int c = ammoList.Count;
            if (!ammoType && c <= 0) return null;

            for (int i = 0; i < c; i++)
            {
                if (ammoList[i].Instance && ammoList[i].Instance.GetInstanceID() == ammoType.GetInstanceID())
                {
                    return ammoList[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the requested amount of ammo of the ammo type.
        /// </summary>
        /// <param name="ammoType">Type of ammunition requested.</param>
        /// <param name="amount">Number of rounds requested by the gun.</param>
        public int RequestAmmunition(AmmoType ammoType, int amount)
        {
            AmmoInstance instance = GetAmmoInstance(ammoType);

            if (instance == null)
                return 0;

            if (instance.Amount >= amount)
            {
                instance.Amount -= amount;
                return amount;
            }

            int remainingAmmo = instance.Amount;
            instance.Amount = 0;
            return remainingAmmo;
        }

        private IEnumerator Interact(IActionable target)
        {
            if (currentWeapon != null && currentWeapon.CanUseEquipment)
            {
                itemCoolDown = true;

                if (target.RequiresAnimation)
                {
                    currentWeapon.Interact();
                    yield return new WaitForSeconds(currentWeapon.InteractDelay);
                }

                target.Interact();

                yield return new WaitForSeconds(Mathf.Max(currentWeapon.InteractAnimationLength - currentWeapon.InteractDelay, 0));
                itemCoolDown = false;
            }
            else if (currentWeapon == null)
            {
                target.Interact();
            }
        }

        /// <summary>
        /// Casts a ray forward trying to find any targetable object in front of the character. 
        /// </summary>
        private void Search()
        {
            Ray ray = new Ray(cameraTransformReference.position, cameraTransformReference.TransformDirection(Vector3.forward));

            RaycastHit[] results = new RaycastHit[4];
            int amount = Physics.SphereCastNonAlloc(ray, fPController.Radius, results, interactionRadius,
                Physics.AllLayers, QueryTriggerInteraction.Collide);

            float dist = interactionRadius;
            GameObject temp = null;

            for (int i = 0, l = results.Length; i < l; i++)
            {
                if (!results[i].collider)
                    continue;

                GameObject c = results[i].collider.gameObject;

                if (c.transform.root == transform.root)
                    continue;

                // Is the object visible?
                if (Physics.Linecast(cameraTransformReference.position, results[i].point, out RaycastHit hitInfo, Physics.AllLayers, QueryTriggerInteraction.Collide))
                {
                    if (hitInfo.collider.gameObject != c)
                        continue;
                }

                // Discard unnecessary objects.
                if (!c.CompareTag(adrenalinePackTag) && !c.CompareTag(ammoTag) && c.GetComponent<IActionable>() == null && c.GetComponent<GunPickup>() == null)
                    continue;

                if (results[i].distance > dist)
                    continue;

                temp = c;
                dist = results[i].distance;
            }

            Target = temp;
        }

        /// <summary>
        /// Switch the weapons the character is equipped with.
        /// </summary>
        /// <param name="current">The current weapon.</param>
        /// <param name="target">The desired weapon.</param>
        /// <param name="currentRounds"></param>
        private IEnumerator Switch(IWeapon current, IWeapon target, int currentRounds)
        {
            current.Deselect();
            yield return new WaitForSeconds(current.HideAnimationLength);

            current.Viewmodel.SetActive(false);
            Select(target, currentRounds);
        }

        /// <summary>
        /// Change the weapons the character is equipped with.
        /// </summary>
        /// <param name="current">The current weapon.</param>
        /// <param name="target">The desired weapon.</param>
        /// <param name="currentRounds"></param>
        private IEnumerator Change(IWeapon current, IWeapon target, int currentRounds)
        {
            current.Deselect();
            if (!fastChangeWeapons)
            {
                yield return new WaitForSeconds(current.HideAnimationLength);
            }

            current.Viewmodel.SetActive(false);
            Select(target, currentRounds);
        }

        /// <summary>
        /// Replace the current weapon for the target weapon and drop it.
        /// </summary>
        /// <param name="current">The current weapon.</param>
        /// <param name="target">The desired weapon.</param>
        /// <param name="drop">The current weapon Prefab.</param>
        /// <param name="currentRounds"></param>
        private IEnumerator DropAndChange(IWeapon current, IWeapon target, GunPickup drop, int currentRounds)
        {
            current.Deselect();
            if (!fastChangeWeapons)
            {
                yield return new WaitForSeconds(((Gun)current).HideAnimationLength);
            }

            if (((Gun)current).DroppablePrefab)
            // ReSharper disable once Unity.InefficientPropertyAccess
            {
                GameObject newGunPickup = Instantiate(((Gun)current).DroppablePrefab, drop.transform.position, drop.transform.rotation);
                newGunPickup.GetComponent<GunPickup>().CurrentRounds = ((Gun)current).CurrentRounds;
            }

            Destroy(drop.transform.gameObject);

            current.Viewmodel.SetActive(false);
            Select(target, currentRounds);
        }

        /// <summary>
        /// Select the target weapon.
        /// </summary>
        /// <param name="weapon">The weapon to be draw.</param>
        /// <param name="currentRounds"></param>
        private void Select(IWeapon weapon, int currentRounds)
        {
            currentWeapon = weapon;
            weapon.SetCurrentRounds(currentRounds);
            weapon.Viewmodel.SetActive(true);
            weapon.Select();
        }

        /// <summary>
        /// Calculates the weight the character is carrying on based on the equipped weapons.
        /// </summary>
        private void CalculateWeight()
        {
            float weight = 0;
            for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
            {
                if (equippedWeaponsList[i] && equippedWeaponsList[i].GetType() == typeof(Gun))
                    weight += equippedWeaponsList[i].Weight;
            }
            fPController.Weight = weight;
        }

        /// <summary>
        /// Makes the character equip a weapon based on its index on the list.
        /// </summary>
        /// <param name="index">The weapon index.</param>
        public void EquipWeapon(int index)
        {
            if (HasFreeSlot)
            {
                for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
                {
                    if (equippedWeaponsList[i])
                        continue;

                    equippedWeaponsList[i] = weaponList[index];
                    return;
                }
            }
        }

        /// <summary>
        /// Makes the character unequip a weapon based on its index on the list.
        /// </summary>
        /// <param name="index"></param>
        public void UnequipWeapon(int index)
        {
            equippedWeaponsList[index] = null;
        }

        /// <summary>
        /// Is the weapon on the Equipped Weapons List?
        /// </summary>
        /// <param name="weapon">The target weapon.</param>
        public bool IsEquipped(IWeapon weapon)
        {
            if (defaultWeapon)
            {
                if (weapon.Identifier == defaultWeapon.Identifier)
                    return true;
            }

            for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
            {
                if (!equippedWeaponsList[i])
                    continue;

                if (equippedWeaponsList[i].Identifier == weapon.Identifier)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetEquippedWeaponIndexOnList(int id)
        {
            for (int i = 0, c = equippedWeaponsList.Count; i < c; i++)
            {
                if (!equippedWeaponsList[i])
                    continue;

                if (equippedWeaponsList[i].Identifier == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetWeaponIndexOnList(int id)
        {
            for (int i = 0, c = weaponList.Count; i < c; i++)
            {
                if (!weaponList[i])
                    continue;

                if (weaponList[i].Identifier == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public IWeapon GetWeaponByID(int id)
        {
            for (int i = 0, c = weaponList.Count; i < c; i++)
            {
                if (!weaponList[i])
                    continue;

                if (weaponList[i].Identifier == id)
                {
                    return weaponList[i];
                }
            }
            return null;
        }
    }
}
