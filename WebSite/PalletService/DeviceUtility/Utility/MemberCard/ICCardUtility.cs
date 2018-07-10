using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;



namespace DeviceUtility.Utility.MemberCard
{
    /// <summary>
    /// �������Ҫ��Ϊ�ǽӴ�ʽ,����Ҫ�Դ������ϴ�Ķ�
    /// �ײ�dll��������ֻ����һ���򵥰�װ,��ֹ�Ķ�̫��
    /// ���뱣���ڿ�3
    /// update by wicky 2015-04-01
    /// </summary>
    public class ICCardUtility
    {

        #region----�ֶ�----
        /// <summary>
        /// ����д��Ŀ�λ��4
        /// </summary>
        private const int ic_postion = 4;
        /// <summary>
        /// ��̬����д��Ŀ�λ��4
        /// </summary>
        private const int ic_pwd_postion = 5;
        /// <summary>
        /// ����д��Ŀ�λ��7
        /// </summary>
        private const int pwd_postion = 7;
        /// <summary>
        /// �Լ�д��Ŀ�����������1
        /// </summary>
        private const int secnr_index = 1;
        /// <summary>
        /// ԭʼIC�������ڵ�����0
        /// </summary>
        private const int secnr_index_ic = 0;
        /// <summary>
        /// �հ׿�0����ʼ����:778852013144
        /// 000000000000FF078069778852013144    
        /// 
        /// 10000602000000000000000000000000    ���ֱ�
        /// 
        /// 10000113000000000000000000000000    ic��
        /// </summary>
        public const string ICPass = "778852013144";
        /// <summary>
        /// 1����ʼ����:FFFFFFFFFFFF
        /// </summary>
        public const string ICPass_one = "FFFFFFFFFFFF";
        /// <summary>
        /// ����ģʽ4
        /// </summary>
        public const int pwd_mode = 4;
        /// <summary>
        /// �հ׿���ʼ����
        /// </summary>
        public static byte[] B_ICPass = new byte[6] { 0x77, 0x88, 0x52, 0x01, 0x31, 0x44 };
        /// <summary>
        /// ���ų���(����̬��)
        /// </summary>
        public static int len = 9;
        #endregion

        #region----�����ײ�dll����----
        /// <summary>
        /// ��ʼ���˿�
        /// </summary>
        private static int IC_InitComm(short Port)
        {
            return Dcrf32.dc_init(Port, 57600);
        }

        /// <summary>
        /// �رն˿�
        /// </summary>
        private static short IC_ExitComm(int icdev)
        {
            return Dcrf32.dc_exit(icdev);
        }

        /// <summary>
        /// ������ͻ�����ؿ������к�
        /// </summary>
        private static short dc_anticoll(int icdev, ref ulong _Snr)
        {
            return Dcrf32.dc_anticoll(icdev, 0, ref _Snr);
        }

        /// <summary>
        /// ���µ�(Ҫ�������������ܼ�������),�˷����Ѿ���Ч
        /// </summary>
        private static short IC_Down(int icdev)
        {
            return 0;
        }

        /// <summary>
        /// ��ʼ������,�˷����Ѿ���Ч
        /// </summary>
        private static short IC_InitType(int icdev, int cardType)
        {
            return 0;
        }

        //�ж������Ƿ�ɹ�,<0 ,���Ӳ��ɹ�.0���Զ�д,1���ӳɹ�,����û�忨.
        private static short IC_Status(int icdev)
        {
            uint tagType = 0;
            Int16 result = Dcrf32.dc_request(icdev, 2, ref tagType); //��ȡ������,Ϊ4,��ʾmifare��
            return result;
        }

        /// <summary>
        /// ��������
        /// </summary>
        private static short IC_DevBeep(int icdev, uint intime)
        {
            return Dcrf32.dc_beep(icdev, intime);
        }

        /// <summary>
        /// ���豸Ӳ���汾��
        /// </summary>
        private static int IC_ReadVer(int icdev, byte[] Databuffer)
        {
            return Dcrf32.dc_getver(icdev, Databuffer);
        }

        /// <summary>
        /// ���ԭʼ����(������0ΪУ��ʧ��)
        /// </summary>
        private static short IC_CheckPass_4442hex(int icdev, string Password, int index = secnr_index)
        {
            Int16 result = 0;
            ulong icCardNo = 0;
            //B_ICPass = new byte[6] { 0x77, 0x88, 0x52, 0x01, 0x31, 0x45 };
            for (int i = 0; i < 6; i++)
            {
                B_ICPass[i] = Convert.ToByte(Password.Substring(i * 2, 2), 16);
            }
            //B_ICPass = Encoding.ASCII.GetBytes(Password); 
            result = Dcrf32.dc_load_key(icdev, pwd_mode, index, B_ICPass);    //������ص��豸
            result = Dcrf32.dc_card(icdev, pwd_mode, ref icCardNo);    //��ȡ����,У��֮ǰ�������
            result = Dcrf32.dc_authentication(icdev, pwd_mode, index);     //У���������
            return result;

        }

        /// <summary>
        /// ��������(0Ϊ���ĳɹ�)
        /// </summary>
        private static short IC_ChangePass_4442hex(int icdev, int pwd_postion, string oldPassWord, string newPassWord)
        {
            Int16 result = 0;
            result = Dcrf32.dc_authentication(icdev, pwd_mode, secnr_index);
            result = ICCardUtility.IC_CheckPass_4442hex(icdev, oldPassWord, secnr_index); //У���������
            if (result != 0) return result;
            string data = newPassWord + "FF078069" + newPassWord;
            result = Dcrf32.dc_write_hex(icdev, pwd_postion, data);
            return result;
        }

        /// <summary>
        /// ��������(0Ϊ���ĳɹ�)
        /// </summary>
        private static short IC_ChangePass(int icdev, int pwd_postion, int secnr_index, string oldPassWord, string newPassWord)
        {
            Int16 result = 0;
            result = Dcrf32.dc_authentication(icdev, pwd_mode, secnr_index);
            result = ICCardUtility.IC_CheckPass_4442hex(icdev, oldPassWord, secnr_index); //У���������
            if (result != 0) return result;
            string data = newPassWord + "FF078069" + newPassWord;
            result = Dcrf32.dc_write_hex(icdev, pwd_postion, data);
            return result;
        }

        /// <summary>
        /// �ڹ̶���λ��д��̶����ȵ�����
        /// </summary>
        private static short IC_Write_hex(int icdev, int offset, int Length, string Databuffer)
        {
            Int16 result = 0;
            result = Dcrf32.dc_authentication(icdev, pwd_mode, getSecNr(offset));    //У���0������;
            if (result != 0) return result;
            string data = addZero(Databuffer, 32, true);
            result = Dcrf32.dc_write_hex(icdev, offset, data);
            return result;
        }
        /// <summary>
        /// �ڹ̶���λ��д��̶����ȵ�����
        /// </summary>
        private static short IC_Write(int icdev, int offset, int Length, byte[] Databuffer)
        {
            Int16 result = 0;
            result = Dcrf32.dc_authentication(icdev, pwd_mode, getSecNr(offset));    //У���0������;
            if (result != 0) return result;
            result = Dcrf32.dc_write(icdev, offset, Databuffer);
            return result;
        }

        /// <summary>
        /// �ڹ̶���λ�ö����̶����ȵ�����
        /// </summary>
        private static short IC_Read(int icdev, int offset, int len, byte[] Databuffer)
        {
            Int16 result = 0;
            result = Dcrf32.dc_authentication(icdev, pwd_mode, getSecNr(offset));    //У���0������;
            if (result != 0) return result;
            result = Dcrf32.dc_read(icdev, offset, Databuffer);     //��ȡ����
            return result;
        }
        /// <summary>
        /// �ڹ̶���λ�ö����̶����ȵ�����
        /// </summary>
        private static short IC_Read_hex(int icdev, int offset, int len, StringBuilder Databuffer)
        {
            Int16 result = 0;
            result = Dcrf32.dc_authentication(icdev, pwd_mode, getSecNr(offset));    //У���0������;
            if (result != 0) return result;
            result = Dcrf32.dc_read_hex(icdev, offset, Databuffer);     //��ȡ����
            return result;
        }
        #endregion

        #region----˽�и�������----
        /// <summary>
        /// ���ݿ��ַ0-63��������0-15
        /// </summary>
        private static int getSecNr(int offset)
        {
            double offsetf = offset;
            double secNr = offsetf / 4.0;
            if (secNr < 0) secNr = 0;
            if (secNr > 63) secNr = 15;
            return (int)secNr;
        }

        /// <summary>
        /// ���ַ���ת��byte[]���ں��油��16�ֽ�
        /// </summary>
        /// <param name="Password"></param>
        /// <returns></returns>
        private static byte[] get16Byte(string Password)
        {
            byte[] a = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] b = Encoding.ASCII.GetBytes(Password);
            for (int i = 0; i < b.Length; i++)
            {
                a[i] = b[i];
            }
            return a;
        }
        /// <summary>
        /// ��0
        /// ������
        ///     str,��Ҫ��0���ַ���
        ///     len,���ص��ַ����ĳ���
        ///     isBack,�Ƿ��ں��油0
        /// </summary>
        private static string addZero(string str, int len, bool isBack = false)
        {
            string temp = str;
            int count = len - temp.Length;
            if (isBack)
            {
                //���油0
                for (int i = 0; i < count; i++)
                {
                    temp += "0";
                }
            }
            else
            {
                string s = "";
                //ǰ�油0
                for (int i = 0; i < count; i++)
                {
                    s += "0";
                }
                temp = s + temp;
            }
            return temp;
        }

        /// <summary>
        /// ����У��,����������IC���Ƿ񶼴���
        /// Ĭ�Ϸ���true
        /// ���ڷ���ok,�����ڷ��ش�����Ϣ
        /// </summary>
        private static string check(int icdev, bool isBeep = true)
        {
            if (icdev <= 0)
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
                return "��ʾ��IC������USB��ʼ��ʧ�ܣ�";
            }
            int iReadDev = -1;
            byte[] sdata = new byte[9];

            iReadDev = ICCardUtility.IC_ReadVer(icdev, sdata);
            if (iReadDev < 0)
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
                return "��ʾ���޷��ҵ�IC������Ӳ���汾�ţ�";
            }
            short st = 0;
            st = ICCardUtility.IC_InitType(icdev, 16);
            if (st < 0)
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
                return "��ʾ������IC������ʧ�ܣ�";
            }
            if (isBeep) st = ICCardUtility.IC_DevBeep(icdev, 10);//�ȴ�10����
            st = ICCardUtility.IC_Status(icdev);
            if (st == 1)
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
                return "��ʾ����������û�в忨";
            }
            if (st != 0)
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
                return "��ʾ���޷����ӵ�������";
            }

            return "ok";
        }

        private static string toASCII(string iccard)
        {
            string str = "";
            byte[] array = System.Text.Encoding.ASCII.GetBytes(iccard);
            for (int i = 0; i < array.Length; i++)
            {
                int asciicode = (int)(array[i]);
                str += Convert.ToString(asciicode);
            }
            return str;
        }
        /// <summary>
        /// ɾ��1�����ݲ��ָ���������FFFFFFFFFFFF
        /// </summary>
        /// <param name="icdev">������ID</param>
        /// <param name="pwd">1������</param>
        /// <returns>�ɹ�true</returns>
        private static bool DeleteICCaredData(int icdev, string pwd)
        {
            byte[] b = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int st = ICCardUtility.IC_CheckPass_4442hex(icdev, pwd);     //�������뵽������
            if (st == 0)
            {
                st = ICCardUtility.IC_Write(icdev, ic_postion, len, b);  //��տ���
                int st1 = ICCardUtility.IC_ChangePass_4442hex(icdev, pwd_postion, pwd, ICPass_one); //�ָ�ԭ��������
                if (st == 0 && st1 == 0)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region----��������----
        /// <summary>
        /// ���0����1�������Ƿ�һ��
        /// </summary>
        public static bool CheckOneTwo(string storePassword)
        {
            string one = GetCardID(0, 1, storePassword);
            string two = GetCardID(1, 4, storePassword);
            if (!one.Equals(two)) return false;
            return true;
        }
        public static string GetUid()
        {
            int icdev = 0;
            icdev = ICCardUtility.IC_InitComm(100);
            string checkStr = check(icdev, false);
            if (!checkStr.Equals("ok")) return checkStr;
            //���IC��UID
            ulong uid = 0;
            int st = dc_anticoll(icdev, ref uid);
            ICCardUtility.IC_ExitComm(icdev);
            return uid.ToString();
        }
        /// <summary>
        /// ��ʼ��0��,����:0���¿��ź�ԭ����
        /// </summary>
        public static string WriteICCard(string card, string pwd)
        {
            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  

                string checkStr = check(icdev);
                if (!checkStr.Equals("ok")) return checkStr;
                int st = ICCardUtility.IC_CheckPass_4442hex(icdev, pwd, 0);     //�������뵽������
                if (st == 0)
                {
                    st = ICCardUtility.IC_Write_hex(icdev, 1, len, card);  //д����
                    int st1 = ICCardUtility.IC_ChangePass(icdev, 3, 0, pwd, ICPass); //д����
                    if (st == 0 && st1 == 0)
                    {
                        return "Success";
                    }
                }
                return "����ʧ��";
            }
            finally
            {
                //�Կ��µ磬�����߼����ܿ����µ����������Ϊ��Ч����Ҫд����������У�����롣
                ICCardUtility.IC_Down(icdev);
                //�رն˿ڣ���Windowsϵͳ�У�ͬʱֻ����һ���豸ʹ�ö˿ڣ��������˳�ϵͳʱ����رն˿ڣ��Ա��������豸ʹ�á�
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ����������
        /// </summary>
        public static void Beep(int time)
        {
            int icdev = 0;
            icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb
            ICCardUtility.IC_DevBeep(icdev, (uint)time);//�ȴ�10����
            ICCardUtility.IC_ExitComm(icdev);
        }
        /// <summary>
        /// �޸Ķ�̬����
        /// </summary>
        public static string ChangeRandomCode(string sNewICCard, string sRepeatCode, string storePassword, bool isBeep = true)
        {
            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  

                string checkStr = "";
                checkStr = check(icdev, isBeep);  //�������

                if (!checkStr.Equals("ok")) return checkStr;

                short st = 0;
                //У���������
                //1���ĳ��������0���������벻ͬ,0��778852013144 1��FFFFFFFFFFFF
                st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass_one);
                if (st != 0)
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword); //У�鱾������
                    if (st != 0)
                    {
                        return "��ʾ���޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                    }
                }
                byte[] b1 = new byte[16];
                byte[] b2 = Encoding.ASCII.GetBytes(sNewICCard);
                Array.Copy(b2, b1, b2.Length);
                b1[b2.Length] = (byte)Convert.ToInt32(sRepeatCode);
                st = ICCardUtility.IC_Write(icdev, ic_postion, len, b1);
                if (st != 0)
                {
                    return "��ʾ���޷�д�붯̬���롣";
                }

                return "Success";
            }
            finally
            {
                //�Կ��µ磬�����߼����ܿ����µ����������Ϊ��Ч����Ҫд����������У�����롣
                ICCardUtility.IC_Down(icdev);
                //�رն˿ڣ���Windowsϵͳ�У�ͬʱֻ����һ���豸ʹ�ö˿ڣ��������˳�ϵͳʱ����رն˿ڣ��Ա��������豸ʹ�á�
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ��ȡ��ǰ�������ϵ�IC������,����8λ�Ŀ��Ż������Ϣ
        /// �˷������д�����+�Ӷ�̬��,�������տ���,��ReadICCard��ͬ
        /// </summary>
        public static bool GetICCardID(string storePassword, out string icCardId, out string repeatCode, out string errMsg, bool isBeep = true, bool isCreate = false)
        {
            icCardId = string.Empty;
            errMsg = string.Empty;
            string sICCardID = string.Empty;
            if (!ICCardUtility.ReadICCard(storePassword, out sICCardID, out repeatCode, out errMsg, isBeep, isCreate))
            {
                return false;
            }

            if (IsValidCardNo(sICCardID))
            {
                try
                {
                    repeatCode = Convert.ToInt32(sICCardID.Substring(8), 16).ToString();
                    sICCardID = sICCardID.Substring(0, 8);
                    int iICCardID = 0;
                    if (ICCardUtility.isNumberic(sICCardID, out iICCardID))
                    {
                        icCardId = iICCardID.ToString();
                    }
                    else
                    {
                        icCardId = sICCardID;
                    }
                    return true;
                }
                catch
                {
                    errMsg = "��ȡ��Ƭ����";
                    return false;
                }
            }
            else
            {
                errMsg = sICCardID;
                return false;
            }

            return true;
        }
        /// <summary>
        /// ������ȡ����(��У��)����������0-15�����0-63
        /// </summary>
        public static string GetCardID(int index, int postion, string storePassword, bool isBeep = false)
        {
            string sICCardID = "";
            string checkStr = "";
            int icdev = 1;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  
                checkStr = check(icdev, isBeep);  //�������
                if (!checkStr.Equals("ok")) return checkStr;

                short st = 0;
                if (index == 0)
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass, index);
                    if (st != 0)
                    {
                        st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass_one, index);
                        if (st != 0) return "��ʾ���޷���ȡIC����Ϣ��";
                    }
                    StringBuilder data = new StringBuilder();
                    st = ICCardUtility.IC_Read_hex(icdev, postion, 8, data); //�¿���4�޿���,��Ϊ����1��ԭʼ����
                    if (st == 0) return data.ToString().Substring(0, 8);
                    return "��ʾ���޷���ȡIC����Ϣ��";
                }
                else
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword, index);
                    if (st != 0)
                    {
                        st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass_one, index);
                        if (st != 0) return "��ʾ���޷���ȡIC����Ϣ��";
                    }
                    byte[] data = new byte[8];
                    st = ICCardUtility.IC_Read(icdev, postion, 8, data);
                    if (st == 0)
                    {
                        sICCardID = Encoding.ASCII.GetString(data).Replace("\0", "0");
                        return sICCardID;
                    }
                    return "��ʾ���޷���ȡIC����Ϣ��";
                }

            }
            finally
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ����������ԱIC����дһ���¿�
        /// </summary>
        public static bool CreateICCard(string sRepeatCode, string storePassword, out string errMsg, bool isBeep = true)
        {
            int icdev = 0;
            errMsg = string.Empty;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  

                string checkStr = "";
                checkStr = check(icdev, isBeep);  //�������

                if (!checkStr.Equals("ok"))
                {
                    errMsg = checkStr;
                    return false;
                }

                short st = 0;

                st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass, secnr_index_ic);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷���ȡIC����Ϣ��";
                    return false;
                }

                StringBuilder data = new StringBuilder();
                st = ICCardUtility.IC_Read_hex(icdev, 1, len - 1, data); //�¿���4�޿���,��Ϊ����1��ԭʼ����
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                    return false;
                }
                //��ȡԭ����
                string sNewICCard = data.ToString().Substring(0, len - 1);
                //У��1��������
                st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass_one, 1);
                if (st != 0)
                {
                    errMsg = "��ʾ���˿�����ʹ�ã��뻻һ�ſհ׿���";
                    return false;
                }
                //д�뿨��
                byte[] b1 = new byte[16];
                byte[] b2 = Encoding.ASCII.GetBytes(sNewICCard);
                Array.Copy(b2, b1, b2.Length);
                b1[b2.Length] = (byte)Convert.ToInt32(sRepeatCode);
                st = ICCardUtility.IC_Write(icdev, ic_postion, len, b1);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�д��IC�����롣";
                    return false;
                }
                //У��1�������룬׼��д�붯̬����                            
                st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass_one, 1);
                if (st != 0)
                {
                    errMsg = "��ʾ���˿�����ʹ�ã��뻻һ�ſհ׿���";
                    return false;
                }
                b1 = new byte[16];
                b1[0] = (byte)Convert.ToInt32(sRepeatCode);
                st = ICCardUtility.IC_Write(icdev, ic_pwd_postion, len, b1);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�д�붯̬���롣";
                    return false;
                }
                st = ICCardUtility.IC_ChangePass_4442hex(icdev, pwd_postion, ICPass_one, storePassword);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�����IC�����롣";
                    return false;
                }
                return true;
            }
            finally
            {
                //�Կ��µ磬�����߼����ܿ����µ����������Ϊ��Ч����Ҫд����������У�����롣
                ICCardUtility.IC_Down(icdev);
                //�رն˿ڣ���Windowsϵͳ�У�ͬʱֻ����һ���豸ʹ�ö˿ڣ��������˳�ϵͳʱ����رն˿ڣ��Ա��������豸ʹ�á�
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ����������ԱIC����дһ���¿�
        /// </summary>
        public static string WriteICCardID(string sNewICCard, string sRepeatCode, string storePassword, bool isBeep = true)
        {
            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  

                string checkStr = "";
                checkStr = check(icdev, isBeep);  //�������

                if (!checkStr.Equals("ok")) return checkStr;

                short st = 0;

                //У���������
                //1���ĳ��������0���������벻ͬ,0��778852013144 1��FFFFFFFFFFFF
                st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass_one);
                if (st != 0)
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword); //У�鱾������
                    if (st != 0)
                    {
                        return "��ʾ���޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                    }
                }
                else
                {
                    st = ICCardUtility.IC_ChangePass_4442hex(icdev, pwd_postion, ICPass_one, storePassword);
                    if (st != 0)
                    {
                        return "��ʾ���޷�����IC�����롣";
                    }
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword); //У�鱾������
                }
                byte[] b1 = new byte[16];
                byte[] b2 = Encoding.ASCII.GetBytes(sNewICCard);
                Array.Copy(b2, b1, b2.Length);
                b1[b2.Length] = (byte)Convert.ToInt32(sRepeatCode);
                st = ICCardUtility.IC_Write(icdev, ic_postion, len, b1);
                if (st != 0)
                {
                    return "��ʾ���޷�д��IC�����롣";
                }
                return "Success";
            }
            finally
            {
                //�Կ��µ磬�����߼����ܿ����µ����������Ϊ��Ч����Ҫд����������У�����롣
                ICCardUtility.IC_Down(icdev);
                //�رն˿ڣ���Windowsϵͳ�У�ͬʱֻ����һ���豸ʹ�ö˿ڣ��������˳�ϵͳʱ����رն˿ڣ��Ա��������豸ʹ�á�
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ���¶�̬����
        /// </summary>
        /// <param name="newRepeatCode">�¶�̬����</param>
        /// <returns></returns>
        public static bool UpdateRepeatCode(string newRepeatCode, string storePassword, out string errMsg)
        {
            string checkStr = "";
            errMsg = string.Empty;
            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  
                checkStr = check(icdev, true);  //�������
                if (!checkStr.Equals("ok"))
                {
                    errMsg = checkStr;
                    return false;
                }

                short st = 0;
                st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword, secnr_index);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                    return false;
                }
                byte[] b1 = new byte[16];
                b1[0] = (byte)Convert.ToInt32(newRepeatCode);
                st = ICCardUtility.IC_Write(icdev, ic_pwd_postion, len, b1);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�д�붯̬���롣";
                    return false;
                }
                return true;
            }
            finally
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ��ȡ16���ƵĿ���(����̬��Ŀ���)
        /// ������isBeep �Ƿ���Ҫ����,
        /// isCreate������������͵Ĳ���,��������հ׿���Ҫ���ؿ���
        /// У���߼���1����豸 2.������� 3.����ǲ��ǿհ׿�
        /// ���ذ�����̬�����������,���¿���ҵ��ֻ���ؿ���
        /// </summary>
        public static bool ReadICCard(string storePassword, out string icCardId, out string repeatCode, out string errMsg, bool isBeep = true, bool isCreate = false)
        {
            string checkStr = "";
            icCardId = string.Empty;
            repeatCode = string.Empty;
            errMsg = string.Empty;
            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  
                checkStr = check(icdev, isBeep);  //�������
                if (!checkStr.Equals("ok"))
                {
                    errMsg = checkStr;
                    return false;
                }

                short st = 0;
                st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword, secnr_index);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                    return false;
                }
                byte[] data = new byte[len];
                st = ICCardUtility.IC_Read(icdev, ic_postion, len, data);
                if (st != 0)
                {
                    errMsg = "��ʾ����Ա���Ŷ�ȡ����";
                    return false;
                }
                byte[] b1 = new byte[len - 1];
                Array.Copy(data, b1, b1.Length);
                byte b2 = data[len - 1];
                icCardId = Encoding.ASCII.GetString(b1) + b2.ToString("X2").Replace("\0", "");  //��ȡ��Ա��
                st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword, secnr_index);
                if (st != 0)
                {
                    errMsg = "��ʾ���޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                    return false;
                }
                st = ICCardUtility.IC_Read(icdev, ic_pwd_postion, len, data);
                if (st != 0)
                {
                    errMsg = "��ʾ����Ա��֤���ȡ����";
                    return false;
                }
                repeatCode = data[0].ToString();    //��ȡ��̬����
                return true;
            }
            finally
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
            }

        }
        /// <summary>
        /// �ж϶������в������һ�ſտ�
        /// </summary>
        public static bool CheckNullCard(string storePassword, bool isBeep = true)
        {
            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  

                if (icdev <= 0)
                {
                    return false;
                }

                int iReadDev = -1;
                byte[] sdata = new byte[9];

                iReadDev = ICCardUtility.IC_ReadVer(icdev, sdata);
                if (iReadDev < 0)
                {
                    return false;
                }

                short st = 0;
                st = ICCardUtility.IC_InitType(icdev, 16);
                if (st < 0)
                {
                    return false;
                }

                if (isBeep)
                {
                    st = ICCardUtility.IC_DevBeep(icdev, 10);//�ȴ�10����
                }

                st = ICCardUtility.IC_Status(icdev);
                if (st == 1)
                {
                    return false;
                }
                if (st != 0)
                {
                    return false;
                }

                st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass);
                if (st != 0)
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword);
                    if (st == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                //�Կ��µ磬�����߼����ܿ����µ����������Ϊ��Ч����Ҫд����������У�����롣
                ICCardUtility.IC_Down(icdev);
                //�رն˿ڣ���Windowsϵͳ�У�ͬʱֻ����һ���豸ʹ�ö˿ڣ��������˳�ϵͳʱ����رն˿ڣ��Ա��������豸ʹ�á�
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ���������տ�
        /// </summary>
        public static bool RecoveryICCard(string storePassword, out string errMsg)
        {
            int icdev = 0;
            errMsg = string.Empty;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  

                string checkStr = check(icdev);
                if (!checkStr.Equals("ok"))
                {
                    errMsg = checkStr;
                    return false;
                }

                if (DeleteICCaredData(icdev, ICPass_one))
                {
                    return true;
                }
                if (DeleteICCaredData(icdev, storePassword))
                {
                    return true;
                }
                if (DeleteICCaredData(icdev, ICPass))
                {
                    return true;
                }
                errMsg = "�˿������¿�Ҳ���Ǳ���Ŀ�,�޷�ͨ������У��";
                return true;
            }
            finally
            {
                //�Կ��µ磬�����߼����ܿ����µ����������Ϊ��Ч����Ҫд����������У�����롣
                ICCardUtility.IC_Down(icdev);
                //�رն˿ڣ���Windowsϵͳ�У�ͬʱֻ����һ���豸ʹ�ö˿ڣ��������˳�ϵͳʱ����رն˿ڣ��Ա��������豸ʹ�á�
                ICCardUtility.IC_ExitComm(icdev);
            }
        }

        /// <summary>
        /// ÿ�β忨ʱ�����µ�IC�����к�
        /// </summary>
        public static string RefreshICCard(string sNewICCard, string sRepeatCode, string storePassword)
        {
            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  

                string checkStr = check(icdev);
                if (!checkStr.Equals("ok")) return checkStr;

                short st = 0;

                st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass);
                if (st != 0)
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword);
                    if (st != 0)
                    {
                        return "�޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                    }
                }
                else
                {
                    return "�˿���һ�ſհ׿�����ˢ�¡�";
                }

                byte[] b1 = new byte[16];
                byte[] b2 = Encoding.ASCII.GetBytes(sNewICCard);
                Array.Copy(b2, b1, b2.Length);
                b1[b2.Length] = (byte)Convert.ToInt32(sRepeatCode);
                st = ICCardUtility.IC_Write(icdev, ic_postion, len, b1);
                if (st != 0)
                {
                    return "�޷�д��IC�����롣";
                }
                return "Success";
            }
            finally
            {
                //�Կ��µ磬�����߼����ܿ����µ����������Ϊ��Ч����Ҫд����������У�����롣
                ICCardUtility.IC_Down(icdev);
                //�رն˿ڣ���Windowsϵͳ�У�ͬʱֻ����һ���豸ʹ�ö˿ڣ��������˳�ϵͳʱ����رն˿ڣ��Ա��������豸ʹ�á�
                ICCardUtility.IC_ExitComm(icdev);
            }
        }
        /// <summary>
        /// ��Ա���������
        /// </summary>
        public static bool isNumberic(string message, out int result)
        {
            //�ж��Ƿ�Ϊ�����ַ���
            //�ǵĻ�����ת��Ϊ���ֲ�������Ϊout���͵����ֵ������true, ����Ϊfalse
            result = -1;   //result ����Ϊout �������ֵ
            try
            {
                //�������ַ�����Ϊ������4ʱ���������ֶ�����ת������ѡһ��
                //���λ������4�Ļ�����ѡ��Convert.ToInt32() ��int.Parse()

                //result = int.Parse(message);
                //result = Convert.ToInt16(message);
                result = Convert.ToInt32(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// ��Ա���������
        /// </summary>
        public static bool isNumberic(string message)
        {
            //�ж��Ƿ�Ϊ�����ַ���
            //�ǵĻ�����ת��Ϊ���ֲ�������Ϊout���͵����ֵ������true, ����Ϊfalse
            int result = -1;
            try
            {
                //�������ַ�����Ϊ������4ʱ���������ֶ�����ת������ѡһ��
                //���λ������4�Ļ�����ѡ��Convert.ToInt32() ��int.Parse()

                //result = int.Parse(message);
                //result = Convert.ToInt16(message);
                result = Convert.ToInt32(message);
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// ����ж�
        /// </summary>
        public static bool isMoney(string message)
        {
            //�ж��Ƿ�Ϊ�����ַ���
            //�ǵĻ�����ת��Ϊ���ֲ�������Ϊout���͵����ֵ������true, ����Ϊfalse
            decimal result = -1;   //result ����Ϊout �������ֵ
            try
            {
                //�������ַ�����Ϊ������4ʱ���������ֶ�����ת������ѡһ��
                //���λ������4�Ļ�����ѡ��Convert.ToInt32() ��int.Parse()

                result = Convert.ToDecimal(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        public static bool IsValidCardNo(string expression)
        {
            if (expression != null)
            {
                string str = expression;
                if (str.Length > 0 && str.Length <= 11 && Regex.IsMatch(str, @"^[-]?[0-9a-zA-Z]*[.]?[0-9a-zA-Z]*$"))
                {
                    if ((str.Length < 10) || (str.Length == 10 && str[0] == '1') || (str.Length == 11 && str[0] == '-' && str[1] == '1'))
                        return true;
                }
            }
            return false;
        }


        public static bool GetNewICCard(string storePassword, out string icCardId, out string errMsg)
        {
            string sICCardID = string.Empty;
            string repeatCode = string.Empty;
            bool isCreate = false;
            bool isBeep = false;
            icCardId = string.Empty;
            errMsg = string.Empty;
            bool isNewCard = false;
            if (!ICCardUtility.ReadNewICCard(storePassword, out isNewCard, out sICCardID, out repeatCode, out errMsg, isBeep, true))
            {
                return false;
            }

            if (IsValidCardNo(sICCardID))
            {
                try
                {
                    string sRepeatCode = Convert.ToInt32(sICCardID.Substring(8), 16).ToString();
                    sICCardID = sICCardID.Substring(0, 8);
                    int iICCardID = 0;
                    if (ICCardUtility.isNumberic(sICCardID, out iICCardID))
                    {
                        icCardId = iICCardID.ToString();
                    }
                    else
                    {
                        icCardId = sICCardID;
                    }
                    return true;
                }
                catch
                {
                    errMsg = "��ȡ��Ƭ����";
                    return false;
                }
            }
            else
            {
                errMsg = sICCardID;
                return false;
            }

            return true;
        }

        public static bool ReadNewICCard(string storePassword, out bool isNewCard, out string icCardId, out string repeatCode, out string errMsg, bool isBeep = true, bool isCreate = true)
        {
            isNewCard = false;
            string checkStr = "";
            icCardId = string.Empty;
            repeatCode = string.Empty;
            errMsg = string.Empty;

            int icdev = 0;
            try
            {
                icdev = ICCardUtility.IC_InitComm(100); //��ʼ��usb  
                checkStr = check(icdev, isBeep);  //�������
                if (!checkStr.Equals("ok"))
                {
                    errMsg = checkStr;
                    return false;
                }

                short st = 0;

                //���¿�
                if (isCreate)
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass, secnr_index_ic);
                    if (st != 0)
                    {
                        errMsg = "��ʾ���޷���ȡIC����Ϣ��";
                        return false;
                    }

                    StringBuilder data = new StringBuilder();
                    st = ICCardUtility.IC_Read_hex(icdev, 1, len - 1, data); //�¿���4�޿���,��Ϊ����1��ԭʼ����
                    if (st == 0)
                    {
                        icCardId = data.ToString().Substring(0, len - 1);
                        repeatCode = new Random().Next(100, 999).ToString();

                        if (CheckNullCard(storePassword, false))
                        {
                            isNewCard = true;
                        }

                        return true;
                    }
                }
                else
                {
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, ICPass_one, secnr_index);
                    if (st == 0)
                    {
                        return true;
                    }
                    st = ICCardUtility.IC_CheckPass_4442hex(icdev, storePassword, secnr_index);
                    if (st != 0)
                    {
                        errMsg = "��ʾ���޷�ͨ������У�飬���ܲ��Ǳ���Ŀ���";
                        return false;
                    }

                    byte[] data = new byte[len];
                    st = ICCardUtility.IC_Read(icdev, ic_postion, len, data);
                    if (st == 0)
                    {
                        byte[] b1 = new byte[len - 1];
                        Array.Copy(data, b1, b1.Length);
                        byte b2 = data[len - 1];
                        string s = Encoding.ASCII.GetString(b1) + b2.ToString("X2");
                        repeatCode = Convert.ToInt32(s.Substring(8), 16).ToString();
                        string sICCardID = s.Substring(0, 8).Replace("\0", "");
                        icCardId = s;
                        return true;

                    }
                }
                errMsg = "��ʾ���޷���ȡIC����Ϣ��";
                return false;
            }
            finally
            {
                ICCardUtility.IC_Down(icdev);
                ICCardUtility.IC_ExitComm(icdev);
            }

        }



    }
}
