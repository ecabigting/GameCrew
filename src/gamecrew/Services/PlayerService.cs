using System;
using System.Collections.Generic;
using gamecrew.Helpers;
using gamecrew.Models;
using MongoDB.Driver;

namespace gamecrew.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IMongoCollection<Player> _players;
        private readonly IMongoCollection<PlayerAccessToken> _playerAccessTransaction;
        private readonly AppSettings AppConf;
        public PlayerService(AppSettings _appConf)
        {
            AppConf = _appConf;
            var mDBClient = new MongoClient(_appConf.ConnectionString);
            var database = mDBClient.GetDatabase(_appConf.DatabaseName);
            _players = database.GetCollection<Player>("Players");
            if (_players == null)
            {
                database.CreateCollection("Players", null);
                _players = database.GetCollection<Player>("Players");
            }
            _playerAccessTransaction = database.GetCollection<PlayerAccessToken>("PlayerAccessToken");
            if (_playerAccessTransaction == null)
            {
                database.CreateCollection("PlayerAccessToken", null);
                _playerAccessTransaction = database.GetCollection<PlayerAccessToken>("PlayerAccessToken");
            }
        }

        public List<Player> Get() =>
            _players.Find(player => true).ToList();

        public Player Get(string id) =>
            _players.Find<Player>(player => player.Id == id).FirstOrDefault();

        public Player Create(Player player)
        {
            player.Password = player.HashP(player.Password, AppConf.Key);
            player.DateEnabled = DateTime.Now;
            player.CreatedDate = DateTime.Now;
            player.LastEditedDate = DateTime.Now;
            player.IsEnabled = true;
            _players.InsertOne(player);
            player = _players.Find<Player>(p => p.Email == player.Email).FirstOrDefault();
            player.IsEnabledBy = player.Id;
            player.CreatedBy = player.Id;
            player.LastEditedBy = player.Id;
            _players.ReplaceOne(p => p.Id == player.Id, player);
            return player;
        }

        public void Update(string id, Player playerIn) =>
            _players.ReplaceOne(player => player.Id == id, playerIn);

        public void Remove(string id) =>
            _players.DeleteOne(player => player.Id == id);

        public Player GetPlayerViaEmail(string email)
        {
            return _players.Find<Player>(p => p.Email == email).FirstOrDefault();
        }

        public bool CheckPassword(Player _player, string inPass)
        {
            if (_player.Password == _player.HashP(inPass, AppConf.Key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    public interface IPlayerService
    {
        List<Player> Get();
        Player Get(string id);
        Player Create(Player player);
        void Update(string id, Player playerIn);
        void Remove(string id);
        Player GetPlayerViaEmail(string email);
        bool CheckPassword(Player player, string inPass);
    }
}