using PLADSE.Servicios.API.RENAPO;
using PLADSE.Servicios.API.RENAPO.serviciosdeconsulta;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsultaCURP = PLADSE.Servicios.API.RENAPO.ConsultaCURP;

namespace _2act
{
    class Program
    {
        static void Main(string[] args)
        {
            string queryPrincipalHccNt = "SELECT nt.idnomina, HCC.num_cheque, HCC.rfc, HCC.t_nomina, ((((right('00' + CONVERT([varchar](2), cod_pago, (0)), (2)) " +
                "+ right('00' + CONVERT([varchar](2), unidad, (0)), (2))) +right('00' + CONVERT([varchar](2), SubUnidad, (0)), (2))) " +
                "+right('' + CONVERT([varchar](7), replace(cat_Puesto, ' ', ''), (0)),(7))) +right('000' + format(horas * 10, 'f0'), (3))) " +
                "+right('000000' + CONVERT([varchar](6), cons_Plaza, (0)),(6)) as plaza, HCC.qna_pago as fecha_pago , " +
                "HCC.qna_ini as fecha_inicio, HCC.qna_fin as fecha_termino, HCC.tot_perc_cheque as percepciones, " +
                "HCC.tot_ded_cheque as deducciones, tot_neto_cheque as neto, " +
                "(((right('00' + CONVERT([varchar](2), HCC.ent_fed, (0)), (2)) + CONVERT([varchar](4), HCC.ct_clasif, (0))) + CONVERT([varchar](4)," +
                "HCC.ct_id,(0)))+right('0000' + CONVERT([varchar](4), HCC.ct_secuencial, (0)),(4)))+HCC.ct_digito_ver as 'cct', " +
                "HCC.tipo_pago, cve_banco = '', HCC.mot_mov as motivo, nivel_cm = 'I', HCC.qna_proc, HCC.num_cheque, HCC.cheque_dv, " +
                "grupo = 'OT' + (right('00' + CONVERT([varchar](2), HCC.cons_qna_proc, (0)), (2))) " +
                "FROM hist_cheque_cpto_c0 HCC left join nominas_timbrado nt " +
                "on(HCC.qna_proc = nt.qna_proc) and(HCC.cons_qna_proc = nt.cons_qna_proc)";

            string queryRfcs_E = "select rfcs.rfc, e.nombre, e.paterno, e.materno " +
                "from(select rfc from hist_cheque_cpto_c0 hcc left " +
                "join nominas_timbrado nt " +
                "on (hcc.qna_proc = nt.qna_proc) and(hcc.cons_qna_proc = nt.cons_qna_proc)) as rfcs " +
                "left join empleado e " +
                "on rfcs.rfc = e.rfc";

            string queryRfcs_Ec = "select rfcs.rfc, ec.cve_unica as curp " +
                "from(select rfc from hist_cheque_cpto_c0 hcc left " +
                "join nominas_timbrado nt " +
                "on (hcc.qna_proc = nt.qna_proc) and(hcc.cons_qna_proc = nt.cons_qna_proc)) as rfcs " +
                "left join empleado_curp as ec " +
                "on rfcs.rfc = ec.rfc";

            string queryRfcs_Enss = "select rfcs.rfc, enss.numero_nss " +
                "from(select rfc from hist_cheque_cpto_c0 hcc " +
                "left join nominas_timbrado nt " +
                "on (hcc.qna_proc = nt.qna_proc) and(hcc.cons_qna_proc = nt.cons_qna_proc)) as rfcs " +
                "left join empleado_nss as enss " +
                "on rfcs.rfc = enss.rfc";

            string queryRfcs_Pec = "select rfcs.rfc, pec.cve_unica " +
                "from(select rfc from hist_cheque_cpto_c0 hcc left " +
                "join nominas_timbrado nt " +
                "on (hcc.qna_proc = nt.qna_proc) and(hcc.cons_qna_proc = nt.cons_qna_proc)) as rfcs " +
                "left join pagos_especiales_curp as pec on rfcs.rfc = pec.rfc";

            List<string> listQuery = new List<string>() { queryPrincipalHccNt, queryRfcs_E, queryRfcs_Ec, queryRfcs_Enss, queryRfcs_Pec };

            AcoplamientoSentencias(queryPrincipalHccNt);
            Console.Read();


        }


        public static void AcoplamientoSentencias(string listquery)
        {
            DataTable dt = new DataTable();
            dt = EjecutarSentencia(listquery);
            Console.WriteLine("Hay algo en el objecto datatable? "+dt.Rows.Count);
            Console.Read();
            /*
            foreach (DataColumn column in dt.Columns)
            {
                Console.WriteLine(column.ColumnName);
            }
            */        
            //dataGrid1.DataSource = table;


            /*DataTable dt2 = EjecutarSentencia(listquery[0].ToString());
            DataTable dt3 = EjecutarSentencia(listquery[0].ToString());
            DataTable dt4 = EjecutarSentencia(listquery[0].ToString());
            DataTable dt5 = EjecutarSentencia(listquery[0].ToString());
            */



        }

 






        public static Boolean CurpValida(String CURP)
        {
            Console.WriteLine("Creo que vmaos bien");
            if (CURP.Length != 18)
                return false;
            ConsultaCURP consultaCURP = new ConsultaCURP();
            try
            {
                ResponseRenapo responseRenapo = consultaCURP.ConsultaPorCurp(CURP);
                if (responseRenapo.ConsultaExitosa == false)
                {
                    Console.WriteLine("-----------------------Curp Invalida : " + CURP);
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().Name + ":" + ex.Message);
                return false;
            }


        }



        public static DateTime QuincenaToFecha(Int32 anioquincena)
        {
            int qna = anioquincena % 100;
            int anio = System.Convert.ToInt32(anioquincena) / 100;
            int mes = System.Convert.ToInt32(qna / 2) + qna % 2;
            int dia = qna % 2 == 0 ? DateTime.DaysInMonth(anio, mes) : 15;

            // Dim dia As Integer = IIf((qna Mod 2) = 0, Date.DaysInMonth(anio, mes), 15)
            string result = String.Format("{0}/{1}/{2}", dia, mes, anio);
            return Convert.ToDateTime(result);
        }


        private static DataTable EjecutarSentencia(string sentencia)
        {
            DataTable tabla = new DataTable();
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                Conexion conn = new Conexion();
                conn.Coneccion();
                conn.GetConnection().Open();
                //conn.getCommand().CommandTimeout = 6000;
                conn.Ejecutar(sentencia);
                SqlDataReader dr = conn.GetCommand().ExecuteReader();
                if (dr.HasRows)
                {
                    try
                    {
                        tabla.Load(dr);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.GetType().Name + ":" + ex.Message);
                    }
                    dr.Close();
                    conn.GetConnection().Close();
                    conn.GetCommand().Dispose();
                    return tabla;
                }
                return tabla;
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "Error Number: " + ex.Errors[i].Number + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n" +
                        "Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
                return null;
            }
        }



        private void InsertTotable(DataTable table)
        {

            try
            {
                string sentencia = "";
                Conexion conn = new Conexion();
                string valor;
                conn.GetConnection().Open();
                //comando.CommandTimeout = 6000;
                conn.Ejecutar(sentencia);
                SqlDataReader dr = conn.GetCommand().ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        try
                        {
                            valor = Convert.ToString(dr["nom_emp"]);
                            Console.Write(valor);
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("Error al establecer la cn}oneccion al servidor !\n" + ex.Message);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al establecer la cn}oneccion al servidor !\n" + ex.Message);
            }
        }

        private static DataTable Filtro(DataTable table)
        {
            DataTable tableOK = new DataTable();
            DataTable tablaNOK = new DataTable();
            SqlDataReader dr;
            Conexion conn = new Conexion();

            conn.Coneccion();
            conn.GetConnection().Open();

            string query = "INSERT INTO [dbo].[anexo_v_pnr]" +
                "([idanexo_v_pnr],[idnomina],[no_comprobante],[ur],[periodo],[tipo_nomina],[primer_apellido],[segundo_apellido],[nombres]," +
                "[clave_plaza],[curp],[rfc],[fecha_pago],[fecha_inicio],[fecha_termino],[percepciones],[deducciones],[neto],[nss],[cct]," +
                "[forma_pago],[cve_banco],[clabe],[motivo],[nivel_cm],[qna_proc],[cons_qna_proc],[num_cheque],[cheque_dv],[grupo])" +
                "VALUES(@idanexo_v_pnr, @idnomina, @no_comprobante, @ur, @periodo, @tipo_nomina, @primer_apellido, @segundo_apellido, " +
                "@nombres, @clave_plaza, @curp, @rfc, @fecha_pago, @fecha_inicio, @fecha_termino, @percepciones, @deducciones, @neto, " +
                "@nss, @cct, @forma_pago, @cve_banco< @clabe, @motivo, @nivel_cm, @qna_proc, @cons_qna_proc, @num_cheque, @cheque_dv, @grupo)";

            conn.Ejecutar(query);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                //table.Rows[i]["cve_unica"].ToString()
                if (CurpValida("aalgo va aca"))
                {
                    //conn.getCommand().Parameters.AddWithValue("")        
                    dr = conn.GetCommand().ExecuteReader();
                    dr.Close();
                }
                conn.GetConnection().Close();
                conn.GetCommand().Dispose();
            }
            return tableOK;
        }

    }

    class Conexion
    {
        private SqlConnection connection = new SqlConnection();
        private SqlCommand command = new SqlCommand();

        public SqlConnection Coneccion()
        {
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                string stringConexion = "data source=winsql;initial catalog=consultalectura;user id=udiaz;password=servicio2022!";
                //string stringConexion = "data source=winsql;initial catalog=consultalectura;user id=udiaz;password=servicio2022!";
                connection = new SqlConnection(stringConexion);
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "Error Number: " + ex.Errors[i].Number + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n" +
                        "Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
            }
            
            return connection;
        }
        public void Ejecutar(string sentencia)
        {
            //Console.WriteLine("Todo bien ?");
            command = new SqlCommand(sentencia, connection);
        }

        public SqlConnection GetConnection() { return connection; }

        public SqlCommand GetCommand() { return command; }

        public void ConnOpen()
        {
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "Error Number: " + ex.Errors[i].Number + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n" +
                        "Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
            }
            
        }

        public void ConnClose()
        {
            connection.Close();
        }
    }
}