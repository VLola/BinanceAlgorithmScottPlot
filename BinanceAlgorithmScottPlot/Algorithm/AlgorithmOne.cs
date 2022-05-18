using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using BinanceAlgorithmScottPlot.Binance;
using BinanceAlgorithmScottPlot.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;

namespace BinanceAlgorithmScottPlot.Algorithm
{
    public static class AlgorithmOne
    {
        public static long CloseOrder(Socket socket, string symbol, long order_id, decimal quantity)
        {
            OrderSide side;
            FuturesOrderType type = FuturesOrderType.Market;
            PositionSide position_side;

            if (order_id != 0)
            {
                position_side = InfoOrderPositionSide(socket, symbol, order_id);
                if (position_side == PositionSide.Long) side = OrderSide.Sell;
                else side = OrderSide.Buy;
                Order(socket, symbol, side, type, quantity, position_side);
                new SoundPlayer(Properties.Resources.wav_1).Play();
                return 0;
            }
            else return order_id;
        }
        public static long OpenOrder(Socket socket, string symbol, decimal quantity, double price_candle, double[] sma)
        {
            OrderSide side;
            FuturesOrderType type = FuturesOrderType.Market;
            PositionSide position_side;
            int count = sma.Length;
            double price_sma = sma[count - 1];
            bool check = true;

            if (sma[count - 2] < sma[count - 3] && sma[count - 3] < sma[count - 4] && sma[count - 4] < sma[count - 5] && sma[count - 5] < sma[count - 6] && sma[count - 6] < sma[count - 7]) check = false;
            else if (sma[count - 2] > sma[count - 3] && sma[count - 3] > sma[count - 4] && sma[count - 4] > sma[count - 5] && sma[count - 5] > sma[count - 6] && sma[count - 6] > sma[count - 7]) check = false;

            if (price_candle < price_sma && check == true)
            {
                position_side = PositionSide.Short;
                side = OrderSide.Sell;
                long order_id = Order(socket, symbol, side, type, quantity, position_side);
                if (order_id != 0) new SoundPlayer(Properties.Resources.wav_2).Play();
                return order_id;
            }
            else if (price_candle > price_sma && check == true)
            {
                position_side = PositionSide.Long; 
                side = OrderSide.Buy;
                long order_id = Order(socket, symbol, side, type, quantity, position_side);
                if (order_id != 0) new SoundPlayer(Properties.Resources.wav_2).Play();
                return order_id;
            }
            return 0;
        }
        public static long Order(Socket socket, string symbol, OrderSide side, FuturesOrderType type, decimal quantity, PositionSide position_side)
        {
            var result = socket.futures.Trading.PlaceOrderAsync(symbol: symbol, side: side, type: type, quantity: quantity, positionSide: position_side).Result;
            if (!result.Success) ErrorText.Add($"Failed OpenOrder: {result.Error.Message}");
            return result.Data.Id;
        }
        public static PositionSide InfoOrderPositionSide(Socket socket, string symbol, long order_id)
        {
            var result = socket.futures.Trading.GetOrderAsync(symbol: symbol, orderId: order_id).Result;
            if (!result.Success)
            {
                ErrorText.Add($"InfoOrderPositionSide: {result.Error.Message}");
                return InfoOrderPositionSide(socket, symbol, order_id);
            }
            return result.Data.PositionSide;
        }
        public static List<BinanceFuturesOrder> InfoOrder(Socket socket, string symbol, DateTime start_time)
        {
            var result = socket.futures.Trading.GetOrdersAsync(symbol: symbol, startTime: start_time).Result;
            if (!result.Success)
            {
                ErrorText.Add($"InfoOrder: {result.Error.Message}");
                return InfoOrder(socket, symbol, start_time);
            }
            return result.Data.ToList();
        }
    }
}
