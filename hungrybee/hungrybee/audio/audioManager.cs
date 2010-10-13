#region using statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace hungrybee
{
    public enum soundType { PLAYER_HURT, ENEMY_KILLED, FRIEND_COLLECTED, GAME_START, JUMP, GAME_END, MENU_UPDOWN, MENU_ENTER, GAME_END_DEATH, PLAYER_FALLING }

    /// <summary>
    /// ***********************************************************************
    /// **                          menuManager                              **
    /// ** Singleton class to hold and store all the menu states.            **
    /// ***********************************************************************
    /// </summary>
    public class audioManager : GameComponent
    {
        #region Local Variables

        game h_game;

        List<soundType> cuedSounds;

        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        public Song menuMusic;
        public bool menuMusicPlaying;
        public Song gameMusic;
        public bool gameMusicPlaying;

        #endregion

        #region Constructor - gameObjectManager(game game)
        /// Initializes to default values
        /// ***********************************************************************
        public audioManager(game game)
            : base(game)
        {
            h_game = (game)game;
            cuedSounds = new List<soundType>();
            menuMusicPlaying = false;
            gameMusicPlaying = false;
        }
        #endregion

        #region Update()
        /// Update()
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Play all the sounds on the cue
            for (int i = 0; i < cuedSounds.Count; i++)
            {
                PlaySound(cuedSounds[i]);
            }

            cuedSounds.Clear();

            // Update the audio engine to make sure all the cues are removed from memory when they have finished
            audioEngine.Update();

            // Nothing to update
            base.Update(gameTime);
        }
        #endregion

        #region Initialize()
        /// Initialize - Nothing to Initialize --> All done in LoadContent()
        /// ***********************************************************************
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region LoadContent()
        /// LoadContent()
        /// ***********************************************************************
        public void LoadContent()
        {
            audioEngine = new AudioEngine(".\\Content\\audio\\hungrybeeAudio.xgs");
            waveBank = new WaveBank(audioEngine, ".\\Content\\audio\\hungrybeeWaveBank.xwb");
            soundBank = new SoundBank(audioEngine, ".\\Content\\audio\\hungrybeeSoundBank.xsb");
            menuMusic = h_game.Content.Load<Song>(".\\audio\\menuMusic");
            gameMusic = h_game.Content.Load<Song>(".\\audio\\menuMusic");

            MediaPlayer.Volume = h_game.h_GameSettings.musicVolume;
        }
        #endregion

        #region PlaySound()
        /// PlaySound() - Get the input sound type and play the correct sound
        /// ***********************************************************************
        protected void PlaySound(soundType sound)
        {
            // Some good sound sources 
            // http://www.opengameart.org/
            // http://www.freesound.org/
            switch (sound)
            {
                case soundType.PLAYER_HURT:
                    //soundBank.PlayCue("laughcartoon");
                    soundBank.PlayCue("bird_squark");
                    break;
                case soundType.ENEMY_KILLED:
                    soundBank.PlayCue("pop");
                    break;
                case soundType.FRIEND_COLLECTED:
                    soundBank.PlayCue("wow");
                    break;
                case soundType.GAME_START:
                    soundBank.PlayCue("gameStart");
                    break;
                case soundType.JUMP:
                    //soundBank.PlayCue("jump_boing");
                    soundBank.PlayCue("grunt_upPitch");
                    break;
                case soundType.GAME_END:
                    soundBank.PlayCue("weee");
                    break;
                case soundType.GAME_END_DEATH:
                    soundBank.PlayCue("Death_sound");
                    break;
                case soundType.PLAYER_FALLING:
                    soundBank.PlayCue("ohh");
                    break;
                case soundType.MENU_UPDOWN:
                    soundBank.PlayCue("menuBeepUpDown");
                    break;
                case soundType.MENU_ENTER:
                    soundBank.PlayCue("menuBeepEnter");
                    break;
                default:
                    throw new Exception("Unrecognized Sound Type");
            }
        }
        #endregion

        #region CueSound()
        /// CueSound() - Add a sound type to the list to be played
        /// ***********************************************************************
        public void CueSound(soundType sound)
        {
            cuedSounds.Add(sound);
        }
        #endregion

        #region PlayMenuMusic()
        /// PlayMenuMusic() - Play the menu music
        /// ***********************************************************************
        public void PlayMenuMusic()
        {
            if (!menuMusicPlaying || gameMusicPlaying)
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(menuMusic);
                MediaPlayer.IsRepeating = true;
                menuMusicPlaying = true;
                gameMusicPlaying = false;
            }
        }
        #endregion

        #region PlayGameMusic()
        /// PlayGameMusic() - Play the menu music
        /// ***********************************************************************
        public void PlayGameMusic()
        {
            if (menuMusicPlaying || !gameMusicPlaying)
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(gameMusic);
                MediaPlayer.IsRepeating = true;
                gameMusicPlaying = true;
                menuMusicPlaying = false;
            }
        }
        #endregion
    }
}
