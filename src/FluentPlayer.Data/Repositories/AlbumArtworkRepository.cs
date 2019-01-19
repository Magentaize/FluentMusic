using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentPlayer.Core.Extensions;
using FluentPlayer.Data.Entities;
using Serilog;
using SQLite;

namespace FluentPlayer.Data.Repositories
{
    public class AlbumArtworkRepository
    {
        private readonly SQLiteAsyncConnection _conn;

        private readonly ILogger _logger;

        public AlbumArtworkRepository(SQLiteAsyncConnectionFactory factory, ILogger logger)
        {
            _conn = factory.GetConnection();
            _logger = logger;
        }

        public async Task DeleteAlbumArtworkAsync(string albumKey)
        {
            try
            {
                await _conn.ExecuteAsync($"DELETE FROM AlbumArtwork WHERE AlbumKey=?;", albumKey);
            }
            catch (Exception ex)
            {
                _logger.Error("Could not delete AlbumArtwork. Exception: {0}", ex.Message);
            }
        }

        public async Task<long> DeleteUnusedAlbumArtworkAsync()
        {
            long unusedAlbumArtworkCount = 0;

            try
            {
                unusedAlbumArtworkCount = await _conn.ExecuteScalarAsync<long>(
                    "SELECT COUNT(AlbumKey) FROM AlbumArtwork WHERE AlbumKey NOT IN (SELECT AlbumKey FROM Track);");
                await _conn.ExecuteAsync(
                    "DELETE FROM AlbumArtwork WHERE AlbumKey NOT IN (SELECT AlbumKey FROM Track);");
            }
            catch (Exception ex)
            {
                _logger.Error("Could not delete unused AlbumArtwork. Exception: {0}", ex.Message);
            }

            return unusedAlbumArtworkCount;
        }

        public async Task<IList<AlbumArtwork>> GetAlbumArtworkAsync()
        {
            IList<AlbumArtwork> albumArtwork = new List<AlbumArtwork>();

            try
            {
                albumArtwork = await _conn.QueryAsync<AlbumArtwork>("SELECT * FROM AlbumArtwork;");
            }
            catch (Exception ex)
            {
                _logger.Error("Could not get album artwork. Exception: {0}", ex.Message);
            }

            return albumArtwork;
        }

        public async Task<AlbumArtwork> GetAlbumArtworkAsync(string albumKey)
        {
            AlbumArtwork albumArtwork = null;

            try
            {
                albumArtwork = (await _conn
                    .QueryAsync<AlbumArtwork>("SELECT * FROM AlbumArtwork WHERE AlbumKey=?;", albumKey))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Error("Could not get album artwork. Exception: {0}", ex.Message);
            }

            return albumArtwork;
        }

        public async Task<AlbumArtwork> GetAlbumArtworkForPathAsync(string path)
        {
            AlbumArtwork albumArtwork = null;

            try
            {
                var albumArtworks = await _conn
                    .QueryAsync<AlbumArtwork>(
                        "SELECT * FROM AlbumArtwork a LEFT JOIN Track t ON a.AlbumKey = t.AlbumKey WHERE t.SafePath=?;",
                        path.ToSafePath());
                albumArtwork = albumArtworks.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not get album artwork for path '{path}'. Exception: {ex.Message}");
            }

            return albumArtwork;
        }

        public async Task<IList<string>> GetArtworkIdsAsync()
        {
            IList<string> artworkIds = new List<string>();

            try
            {
               artworkIds = (await _conn.QueryAsync<AlbumArtwork>("SELECT * FROM AlbumArtwork;"))
                    .Select(a => a.ArtworkId).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Could not get artwork id's. Exception: {0}", ex.Message);
            }

            return artworkIds;
        }

        public async Task UpdateAlbumArtworkAsync(string albumKey, string artworkId)
        {
            try
            {
                await _conn.ExecuteAsync("DELETE FROM AlbumArtwork WHERE AlbumKey=?;", albumKey);
                await _conn.ExecuteAsync("INSERT INTO AlbumArtwork(AlbumKey, ArtworkID) VALUES(?, ?);", albumKey,
                    artworkId);
            }
            catch (Exception ex)
            {
                _logger.Error("Could not update AlbumArtwork. Exception: {0}", ex.Message);
            }
        }
    }
}