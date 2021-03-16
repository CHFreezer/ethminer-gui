using System;

namespace EthminerGUI
{
    public struct Miner
    {
        public enum Name
        {
            Ethminer = 0,
            PhoenixMiner,
            NBMiner
        }

        public Name name;
        public string exePath;
        public string pool;
        public string wallet;
        public string passwd;
        public string pool2;
        public string wallet2;
        public string passwd2;
        public string args;

        string getEthminerPool(string @pool, string @wallet, string @passwd)
        {
            var poolArr = @pool.Split(new string[] { "://" }, StringSplitOptions.None);
            string protocol = "";
            string url;
            if (poolArr.Length < 2)
            {
                url = poolArr[0].Trim();
            }
            else
            {
                protocol = poolArr[0].Trim();
                url = poolArr[1].Trim();
            }

            return (string.IsNullOrWhiteSpace(protocol) ? "" : $"{protocol}://") +
                   @wallet.Trim() +
                   (string.IsNullOrWhiteSpace(App.Configuration.LocalMachineName) ? "" : $".{App.Configuration.LocalMachineName}") +
                   (string.IsNullOrWhiteSpace(@passwd) ? "" : $":{@passwd.Trim()}") +
                   $"@{url}";
        }

        string getNbminerUser(string @wallet, string @passwd)
        {
            return @wallet.Trim() +
                   (string.IsNullOrWhiteSpace(App.Configuration.LocalMachineName) ? "" : $".{App.Configuration.LocalMachineName}") +
                   (string.IsNullOrWhiteSpace(@passwd) ? "" : $":{@passwd.Trim()}");
        }

        string getPool()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return $"-P {getEthminerPool(pool, wallet, passwd)} ";
                case Name.PhoenixMiner:
                    return $"-pool {pool.Trim()} ";
                case Name.NBMiner:
                    return $"-o {pool.Trim()} ";
                default:
                    return "";
            }
        }
        string getPool2()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return (string.IsNullOrWhiteSpace(pool2) || string.IsNullOrWhiteSpace(wallet2)) ?
                           "" : $"-P {getEthminerPool(pool2, wallet2, passwd2)} ";
                case Name.PhoenixMiner:
                    return string.IsNullOrWhiteSpace(pool2) ? "" : $"-pool2 {pool2.Trim()} ";
                case Name.NBMiner:
                    return string.IsNullOrWhiteSpace(pool2) ? "" : $"-o1 {pool2.Trim()} ";
                default:
                    return "";
            }
        }

        string getWallet()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return "";
                case Name.PhoenixMiner:
                    return $"-wal {wallet.Trim()} ";
                case Name.NBMiner:
                    return $"-u {getNbminerUser(wallet, passwd)} ";
                default:
                    return "";
            }
        }
        string getWallet2()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return "";
                case Name.PhoenixMiner:
                    return (string.IsNullOrWhiteSpace(pool2) || string.IsNullOrWhiteSpace(wallet2)) ?
                           "" : $"-wal2 {wallet.Trim()} ";
                case Name.NBMiner:
                    return (string.IsNullOrWhiteSpace(pool2) || string.IsNullOrWhiteSpace(wallet2)) ?
                           "" : $"-u1 {getNbminerUser(wallet2, passwd2)} ";
                default:
                    return "";
            }
        }

        string getWorker()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return "";
                case Name.PhoenixMiner:
                    return string.IsNullOrWhiteSpace(App.Configuration.LocalMachineName) ?
                        "" : $"-worker {App.Configuration.LocalMachineName} ";
                case Name.NBMiner:
                    return "";
                default:
                    return "";
            }
        }
        string getWorker2()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return "";
                case Name.PhoenixMiner:
                    return (string.IsNullOrWhiteSpace(pool2) || string.IsNullOrWhiteSpace(wallet2) || string.IsNullOrWhiteSpace(App.Configuration.LocalMachineName)) ?
                        "" : $"-worker2 {App.Configuration.LocalMachineName} ";
                case Name.NBMiner:
                    return "";
                default:
                    return "";
            }
        }

        string getPassword()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return "";
                case Name.PhoenixMiner:
                    return string.IsNullOrWhiteSpace(passwd) ? "" : $"-pass {passwd.Trim()} ";
                case Name.NBMiner:
                    return "";
                default:
                    return "";
            }
        }
        string getPassword2()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return "";
                case Name.PhoenixMiner:
                    return (string.IsNullOrWhiteSpace(pool2) || string.IsNullOrWhiteSpace(wallet2) || string.IsNullOrWhiteSpace(passwd2)) ?
                           "" : $"-pass2 {passwd2.Trim()} ";
                case Name.NBMiner:
                    return "";
                default:
                    return "";
            }
        }

        public string GetFullArgs()
        {
            switch (name)
            {
                case Name.Ethminer:
                    return $"{getPool()}{getPool2()}{args.Trim()}";
                case Name.PhoenixMiner:
                    return $"{getPool()}{getWallet()}{getWorker()}{getPassword()}" +
                           $"{getPool2()}{getWallet2()}{getWorker2()}{getPassword2()}" +
                           $"-coin eth -log 0 -wdog 0 -rmode 0 {args.Trim()}";
                case Name.NBMiner:
                    return $"-a ethash {getPool()}{getWallet()}" +
                           $"{getPool2()}{getWallet2()}" +
                           $"--no-watchdog {args.Trim()}";
                default:
                    throw new Exception("不支持的挖矿内核");
            }
        }
    }
}
