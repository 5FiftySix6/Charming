﻿using System.IO;
using Modding;
using UnityEngine;
using ModCommon;

namespace CharmingMod
{
    using Components;
    /* 
     * For a nicer building experience, change 
     * SET MOD_DEST="K:\Games\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Mods"
     * in install_build.bat to point to your hollow knight mods folder...
     * 
     */
    public partial class CharmingMod : Mod<CharmingModSaveSettings, CharmingModSettings>, ITogglableMod
    {  
        public static CharmingMod Instance { get; private set; }

        CommunicationNode comms;

        public override void Initialize()
        {
            if(Instance != null)
            {
                Log("Warning: "+this.GetType().Name+" is a singleton. Trying to create more than one may cause issues!");
                return;
            }

            Instance = this;
            comms = new CommunicationNode();
            comms.EnableNode( this );

            Log( this.GetType().Name +" initializing!");

            SetupDefaulSettings();

            UnRegisterCallbacks();
            RegisterCallbacks();

            Log( this.GetType().Name + " is done initializing!" );
        }

        void SetupDefaulSettings()
        {
            string globalSettingsFilename = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = false;
            if( GlobalSettings != null && GlobalSettings.SettingsVersion != CharmingSettingsVars.GlobalSettingsVersion )
            {
                forceReloadGlobalSettings = true;
            }
            else
            {
                Log( "Global settings version match!" );
            }

            if( forceReloadGlobalSettings || !File.Exists( globalSettingsFilename ) )
            {
                if( forceReloadGlobalSettings )
                {
                    Log( "Global settings are outdated! Reloading global settings" );
                }
                else
                {
                    Log( "Global settings file not found, generating new one... File was not found at: " + globalSettingsFilename );
                }

                GlobalSettings.Reset();

                GlobalSettings.SettingsVersion = CharmingSettingsVars.GlobalSettingsVersion;
            }

            SaveGlobalSettings();
            Dev.Log( "Mod done setting initializing!" );
        }

        ///Revert all changes the mod has made
        public void Unload()
        {
            UnRegisterCallbacks();
            comms.DisableNode();
            Instance = null;
        }

        //TODO: update when version checker is fixed in new modding API version
        public override string GetVersion()
        {
            return CharmingSettingsVars.ModVersion;
        }

        //TODO: update when version checker is fixed in new modding API version
        public override bool IsCurrent()
        {
            return true;
        }

        void RegisterCallbacks()
        {
            Dev.Where();
            ModHooks.Instance.SlashHitHook -= DebugPrintObjectOnHit; 
            ModHooks.Instance.SlashHitHook += DebugPrintObjectOnHit;
        }

        void UnRegisterCallbacks()
        {
            Dev.Where(); 
            ModHooks.Instance.SlashHitHook -= DebugPrintObjectOnHit;
        }

        static string debugRecentHit = "";
        //static PhysicsMaterial2D hbMat;
        static void DebugPrintObjectOnHit( Collider2D otherCollider, GameObject gameObject )
        {
            //Dev.Where();
            if( otherCollider.gameObject.name != debugRecentHit )
            {
                //Dev.Log( "Hero at " + HeroController.instance.transform.position + " HIT: " + otherCollider.gameObject.name + " at (" + otherCollider.gameObject.transform.position + ")" + " with layer (" + otherCollider.gameObject.layer + ")" );
                debugRecentHit = otherCollider.gameObject.name;
            }

            //TODO: something in here throws a nullref
            
            if( !HeroController.instance.playerData.equippedCharm_15 )
                return;

            Rigidbody2D body = otherCollider.GetComponentInParent<Rigidbody2D>();

            if( body == null )
                return;

            bool isEnemy = body.gameObject.IsGameEnemy();
            if( !isEnemy )
                return;

            TakeDamageFromImpact dmgOnImpact = body.gameObject.GetOrAddComponent<TakeDamageFromImpact>();
            PreventOutOfBounds poob = body.gameObject.GetOrAddComponent<PreventOutOfBounds>();
            DamageEnemies dmgEnemies = body.gameObject.GetOrAddComponent<DamageEnemies>();

            Vector2 blowDirection = otherCollider.transform.position - HeroController.instance.transform.position;
            float blowPower = 40f;

            dmgOnImpact.blowVelocity = blowDirection.normalized * blowPower;            
            dmgEnemies.damageDealt = (int)blowPower;
        }
    }
}
