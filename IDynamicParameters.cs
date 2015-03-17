/*
 * Author  : sfk 
 * Licence : Open Source
*/

using System.Data;

namespace DbConnect
{
    /// <summary>
    /// Interface used to call methods ( AddParameters,CallBack ) from the Parameters object sent by client
    /// </summary>
    internal interface IDynamicParameters
    {
        /// <summary>
        /// Adds parameters to Command Object before executing the query
        /// </summary>
        /// <param name="cmd">Command Object</param>
        void AddParameters(IDbCommand cmd);

        /// <summary>
        /// Callback function triggered after executing the query to collect the out params
        /// </summary>
        /// <param name="cmd">Command Object</param>
        void CallBack(IDbCommand cmd);
    }
}