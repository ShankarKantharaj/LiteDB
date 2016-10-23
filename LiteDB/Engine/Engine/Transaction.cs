﻿using System;

namespace LiteDB
{
    // Transactions are enabled by default. All operations occurs first in memory only (pages) and will be persist
    // only when Commit calls or LiteEngine is dispoable. 
    public partial class LiteEngine : IDisposable
    {
        /// <summary>
        /// Commit current transaction
        /// </summary>
        public void Commit()
        {
            using (_locker.Write())
            {
                _trans.Commit();
            }
        }

        /// <summary>
        /// Rollback current transaction and restore database from last commit
        /// </summary>
        public void Rollback()
        {
            using (_locker.Write())
            {
                _trans.Rollback();
            }
        }

        /// <summary>
        /// Encapsulate all write transaction operation
        /// </summary>
        private T Transaction<T>(string colName, bool addIfNotExists, Func<CollectionPage, T> action)
        {
            using(_locker.Write())
            {
                try
                {
                    var col = this.GetCollectionPage(colName, addIfNotExists);

                    var result = action(col);

                    if (_autocommit) _trans.Commit();

                    return result;
                }
                catch (Exception ex)
                {
                    _log.Write(Logger.ERROR, ex.Message);
                    // if an error occurs during an operation, rollback must be called to avoid datafile inconsistent
                    _trans.Rollback();
                    throw;
                }
            }
        }
    }
}