using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using carp_tm_2025.Data;
using carp_tm_2025.Models;
using iText.Kernel.Pdf;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using X.Web.PagedList;
using X.PagedList.Extensions;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.Kernel.Geom;
using iText.Kernel.Events;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Layout;
using iText.Layout.Renderer;
using iText.Layout.Borders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.FlowAnalysis;
using iText.Kernel.Utils.Annotationsflattening;
using Mono.TextTemplating;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace carp_tm_2025.Controllers
{
    [Authorize(Roles = "CARP_TM\\CARP_TM_READ")]
    public class registru_jurnalController : Controller
    {
        private readonly carp_tm_2025Context _context;

        private readonly IWebHostEnvironment _env;

        public static string titlu1_raport_rj;


        public registru_jurnalController(carp_tm_2025Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;

        }

        // GET: registru_jurnal
        public async Task<ActionResult> Index(string ziua_rj = "01/01/2015", string luna_rj = "0", int tip_selectie = 1, string nrcont_rj = "0", int id_cont_analitic_rj = 0, int id_cont_sintetic_rj = 0, string nr_document = "0",  int? page = 1)
        {
            //tbl sept 2024
            DateTime ziuaOK_rj = Convert.ToDateTime("01/01/2001");
            int lunaOK_rj = 0;
            int an_rj = 2025;
            int nrcontOK_rj = 0;

            string pdfrjIndex = "";

            int pageSize = 15;
            

            decimal total_credit = 0;
            decimal total_debit = 0;

            decimal total_credit_A = 0;
            decimal total_debit_A = 0;

            decimal SF_rj = 0;
            decimal SI_rj = 0;

            int id_cont_CS_selectat = 0;
            int id_cont_CA_selectat = 0;

            string nr_cont = "0";


            Boolean activ = false;

            string TitluPerioadaRJ = "-";

            string extract_nrcarnet = "-";

            double SI_anterior = 0;
            decimal sold_initial_CA = 0;

            int pens_nrcarnet_rj = 0;
            //de aici pt centr

            lunaOK_rj = Int32.Parse(luna_rj);



            if (nr_document.IsNullOrEmpty()) nr_document= "0";

            if (tip_selectie == 3) tip_selectie = 2;

            var registru_jurnal =  _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                .Where(rj => rj.id_registru_jurnal == 10);

            var registru_jurnal_sum =  _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
             .Where(rj => rj.id_registru_jurnal == 10);

            var get_si_rj =  _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
              .Where(rj => rj.id_registru_jurnal == 10);

            var get_si_rj_sum = get_si_rj.Where(gsirj => gsirj.id_CS_creditor == 2);




            if (lunaOK_rj == 0)
            {
                if (DateTime.TryParse(ziua_rj, out ziuaOK_rj) == false)
                {
                    ViewData["Error"] = "Introduceti o valoare calendaristica pentru ziua";
                    return View("Error");

                }


                ViewData["ziua_rj"] = ziuaOK_rj;

                get_si_rj =  _context.registru_jurnal.Where(gsirj => gsirj.data < ziuaOK_rj);

                registru_jurnal =  _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                       .Where(rj => (rj.data.Day == ziuaOK_rj.Day) && (rj.data.Month == ziuaOK_rj.Month) && (rj.data.Year == ziuaOK_rj.Year)).OrderBy(rj => rj.id_registru_jurnal);
            }

            if (lunaOK_rj != 0)
            {
                if (lunaOK_rj != 13)
                {
                    get_si_rj =  _context.registru_jurnal.Where(gsirj => gsirj.data.Month < lunaOK_rj);
                    registru_jurnal =  _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                      .Where(rj => (rj.data.Month == lunaOK_rj) && (rj.data.Year == an_rj)).OrderBy(rj => rj.id_registru_jurnal);
                }

                if (lunaOK_rj == 13)
                {
                    if (id_cont_CS_selectat == 51)
                    {
                        ViewData["Error"] = "Nu se poate face deocamdata situatia anuala pentru contul 5311";
                        return View("Error");
                    }

                    get_si_rj =  _context.registru_jurnal.Where(gsirj => gsirj.data.Month == 13);

                    registru_jurnal =  _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                                    .Where(rj => (rj.data.Year == an_rj)).OrderBy(rj => rj.id_registru_jurnal);
                }

            }


            //de aici ndoc




            if (tip_selectie == 1) registru_jurnal = registru_jurnal.OrderByDescending(rj => rj.data);

            //pa092024cod
            if (tip_selectie == 2)
            {
                if (id_cont_analitic_rj != 0)
                {
                    registru_jurnal = registru_jurnal
                      .Where(rj => (rj.id_CA_debitor == id_cont_analitic_rj) || (rj.id_CA_creditor == id_cont_analitic_rj)).OrderBy(rj => rj.id_registru_jurnal);
                    get_si_rj = get_si_rj.Where(gsirj => (gsirj.id_CA_debitor == id_cont_analitic_rj) || (gsirj.id_CA_creditor == id_cont_analitic_rj)).OrderBy(gsirj => gsirj.id_registru_jurnal);
                    id_cont_CA_selectat = id_cont_analitic_rj;

                    var getSI =  _context.solduri_initiale.Where(gsi => gsi.id_CA_SI == 0).SingleOrDefault();
                    getSI =  _context.solduri_initiale.Where(gsi => (gsi.id_CA_SI == id_cont_analitic_rj) && (gsi.an==DateTime.Now.Year-1)).SingleOrDefault();
                    if (getSI != null) sold_initial_CA = getSI.SI;
                    if (getSI == null) sold_initial_CA = 0;


                   
                    var get_IDCA_rjidLista = _context.conturi_analitice.Where(gidcal => gidcal.id_cont_analitic == id_cont_analitic_rj).SingleOrDefault();

                    if ((get_IDCA_rjidLista.id_cont_sintetic == 18) || (get_IDCA_rjidLista.id_cont_sintetic == 26) || (get_IDCA_rjidLista.id_cont_sintetic == 27))
                    {

                        if (get_IDCA_rjidLista.nr_cont_analitic.Trim().Length == 6) extract_nrcarnet = get_IDCA_rjidLista.nr_cont_analitic.Substring(5, 1);
                        if (get_IDCA_rjidLista.nr_cont_analitic.Trim().Length == 7) extract_nrcarnet = get_IDCA_rjidLista.nr_cont_analitic.Substring(5, 2);
                        if (get_IDCA_rjidLista.nr_cont_analitic.Trim().Length == 8) extract_nrcarnet = get_IDCA_rjidLista.nr_cont_analitic.Substring(5, 3);
                        if (get_IDCA_rjidLista.nr_cont_analitic.Trim().Length == 9) extract_nrcarnet = get_IDCA_rjidLista.nr_cont_analitic.Substring(5, 4);
                        if (get_IDCA_rjidLista.nr_cont_analitic.Trim().Length == 10) extract_nrcarnet =get_IDCA_rjidLista.nr_cont_analitic.Substring(5, 5);

                        int pens_nrcarnet_rjl;
                        pens_nrcarnet_rjl = 0;
                        pens_nrcarnet_rjl = Int32.Parse(extract_nrcarnet);
                        if ((get_IDCA_rjidLista.id_cont_sintetic == 26) || (get_IDCA_rjidLista.id_cont_sintetic == 27)) pens_nrcarnet_rjl = id_cont_analitic_rj;


                        var pensGetSIL = _context.Pensionars.Where(pgsil => pgsil.nrcarnet == pens_nrcarnet_rjl).SingleOrDefault();

                        sold_initial_CA = 0;                                     

                        if (get_IDCA_rjidLista.id_cont_sintetic == 18) sold_initial_CA = pensGetSIL.sold_462_2016;
                        if (get_IDCA_rjidLista.id_cont_sintetic == 26) sold_initial_CA = pensGetSIL.sold_461_2016;
                        if (get_IDCA_rjidLista.id_cont_sintetic == 27) sold_initial_CA = pensGetSIL.sold_461_2016;

                        
                    }
                    


                }

                if (id_cont_analitic_rj == 0)
                    if (id_cont_sintetic_rj != 0)
                    {
                        registru_jurnal = registru_jurnal
                          .Where(rj => (rj.id_CS_debitor == id_cont_sintetic_rj) || (rj.id_CS_creditor == id_cont_sintetic_rj)).OrderBy(rj => rj.id_registru_jurnal);
                        get_si_rj = get_si_rj.Where(gsirj => (gsirj.id_CS_debitor == id_cont_sintetic_rj) || (gsirj.id_CS_creditor == id_cont_sintetic_rj)).OrderBy(gsirj => gsirj.id_registru_jurnal);
                        id_cont_CS_selectat = id_cont_sintetic_rj;
                    }

                if (id_cont_analitic_rj == 0)
                    if (id_cont_sintetic_rj == 0)
                    {
                        var get_IDCA_rj =  _context.conturi_analitice.Where(gicarj => gicarj.nr_cont_analitic == "421.1").SingleOrDefault();
                        var get_IDCS_rj =  _context.conturi_sintetice.Where(gicsrj => gicsrj.nr_cont_sintetic == 2).SingleOrDefault();


                        if (nrcont_rj.Contains("."))
                        {
                            get_IDCA_rj =  _context.conturi_analitice.Where(gicarj => gicarj.nr_cont_analitic == nrcont_rj).SingleOrDefault();


                            if (get_IDCA_rj == null)
                            {
                                ViewData["Error"] = "Nu exista acest nr de cont";
                                return View("Error");
                            }

                            if (get_IDCA_rj != null)
                                if (get_IDCA_rj.id_cont_analitic < 1100)
                                {
                                    ViewData["Error"] = "Acest tip de cont se selecteaza din lista";
                                    return View("Error");
                                }

                            if (get_IDCA_rj != null)
                                registru_jurnal = registru_jurnal
                                  .Where(rj => (rj.id_CA_debitor == get_IDCA_rj.id_cont_analitic) || (rj.id_CA_creditor == get_IDCA_rj.id_cont_analitic)).OrderBy(rj => rj.id_registru_jurnal);

                            get_si_rj = get_si_rj.Where(gsirj => (gsirj.id_CA_debitor == get_IDCA_rj.id_cont_analitic) || (gsirj.id_CA_creditor == get_IDCA_rj.id_cont_analitic)).OrderBy(rj => rj.id_registru_jurnal);

                            id_cont_CA_selectat = get_IDCA_rj.id_cont_analitic;

                            //de aici
                            if ((get_IDCA_rj.id_cont_sintetic == 103) || (get_IDCA_rj.id_cont_sintetic == 106) || (get_IDCA_rj.id_cont_sintetic == 107) || (get_IDCA_rj.id_cont_sintetic == 108) || (get_IDCA_rj.id_cont_sintetic == 109))
                            {
                                if (nrcont_rj.Length == 6) extract_nrcarnet = nrcont_rj.Substring(5, 1);
                                if (nrcont_rj.Length == 7) extract_nrcarnet = nrcont_rj.Substring(5, 2);
                                if (nrcont_rj.Length == 8) extract_nrcarnet = nrcont_rj.Substring(5, 3);
                                if (nrcont_rj.Length == 9) extract_nrcarnet = nrcont_rj.Substring(5, 4);
                                if (nrcont_rj.Length == 10) extract_nrcarnet = nrcont_rj.Substring(5, 5);
                            }

                            if ((get_IDCA_rj.id_cont_sintetic == 101) || (get_IDCA_rj.id_cont_sintetic == 102))
                            {
                                if (nrcont_rj.Length == 5) extract_nrcarnet = nrcont_rj.Substring(4, 1);
                                if (nrcont_rj.Length == 6) extract_nrcarnet = nrcont_rj.Substring(4, 2);
                                if (nrcont_rj.Length == 7) extract_nrcarnet = nrcont_rj.Substring(4, 3);
                                if (nrcont_rj.Length == 8) extract_nrcarnet = nrcont_rj.Substring(4, 4);
                                if (nrcont_rj.Length == 9) extract_nrcarnet = nrcont_rj.Substring(4, 5);

                            }

                            if ((get_IDCA_rj.id_cont_sintetic == 108) || (get_IDCA_rj.id_cont_sintetic == 109))
                            {
                                if (nrcont_rj.Length == 7) extract_nrcarnet = nrcont_rj.Substring(6, 1);
                                if (nrcont_rj.Length == 8) extract_nrcarnet = nrcont_rj.Substring(6, 2);
                                if (nrcont_rj.Length == 9) extract_nrcarnet = nrcont_rj.Substring(6, 3);
                                if (nrcont_rj.Length == 10) extract_nrcarnet = nrcont_rj.Substring(6, 4);
                                if (nrcont_rj.Length == 11) extract_nrcarnet = nrcont_rj.Substring(6, 5);

                            }


                            if ((get_IDCA_rj.id_cont_sintetic == 18) || (get_IDCA_rj.id_cont_sintetic == 26) || (get_IDCA_rj.id_cont_sintetic == 27))
                            {
                                if (nrcont_rj.Length == 6) extract_nrcarnet = nrcont_rj.Substring(5, 1);
                                if (nrcont_rj.Length == 7) extract_nrcarnet = nrcont_rj.Substring(5, 2);
                                if (nrcont_rj.Length == 8) extract_nrcarnet = nrcont_rj.Substring(5, 3);
                                if (nrcont_rj.Length == 9) extract_nrcarnet = nrcont_rj.Substring(5, 4);
                                if (nrcont_rj.Length == 10) extract_nrcarnet = nrcont_rj.Substring(5, 5);

                            }


                            sold_initial_CA = 0;
                            pens_nrcarnet_rj = Int32.Parse(extract_nrcarnet);

                            var pensGetSI =  _context.Pensionars.Where(pgsi => pgsi.nrcarnet == pens_nrcarnet_rj).SingleOrDefault();
                            if (get_IDCA_rj.id_cont_sintetic == 103) sold_initial_CA = pensGetSI.soldimp2016 - pensGetSI.debit_imprumut2016;
                            if (get_IDCA_rj.id_cont_sintetic == 106) sold_initial_CA = pensGetSI.credit_imprumut2016;
                            if (get_IDCA_rj.id_cont_sintetic == 107) sold_initial_CA = pensGetSI.debit_imprumut2016;
                            if (get_IDCA_rj.id_cont_sintetic == 109) sold_initial_CA = pensGetSI.sold_dobanda2016;
                            if (get_IDCA_rj.id_cont_sintetic == 109) sold_initial_CA = pensGetSI.sold_dobanda2016;
                            //if ((get_IDCA_rj.id_cont_sintetic == 109) && (pensGetSI.id_stare==4)) sold_initial_CA = pensGetSI.sold_dobanda2016;


                            if (get_IDCA_rj.id_cont_sintetic == 108) sold_initial_CA = pensGetSI.sold_DP2016;
                            if (get_IDCA_rj.id_cont_sintetic == 101) sold_initial_CA = pensGetSI.soldcotiz2016;
                            if (get_IDCA_rj.id_cont_sintetic == 102) sold_initial_CA = pensGetSI.soldajdeces2016;
                            if (get_IDCA_rj.id_cont_sintetic == 18)  sold_initial_CA = pensGetSI.sold_462_2016;
                            if (get_IDCA_rj.id_cont_sintetic == 26)  sold_initial_CA = pensGetSI.sold_461_2016;
                            if (get_IDCA_rj.id_cont_sintetic == 27)  sold_initial_CA = pensGetSI.sold_461_2016;

                            if (pens_nrcarnet_rj == 30000) sold_initial_CA = 31;
                            if (pens_nrcarnet_rj == 30001) sold_initial_CA = 42;
                            if (pens_nrcarnet_rj == 30002) sold_initial_CA = 540;
                            if (pens_nrcarnet_rj == 30003) sold_initial_CA = 245.14M;
                            if (pens_nrcarnet_rj == 74002) sold_initial_CA =300;

                            int nrcDP = 0;
                            nrcDP=Int32.Parse("4611" + extract_nrcarnet.ToString());
                            if ((get_IDCA_rj.id_cont_sintetic == 108) || (get_IDCA_rj.id_cont_sintetic == 109))
                            {
                                pensGetSI = _context.Pensionars.Where(pgi => pgi.nrcarnet == nrcDP).SingleOrDefault();
                               // if ((pensGetSI != null) && (get_IDCA_rj.id_cont_sintetic == 109)) sold_initial_CA = pensGetSI.sold_dobanda2016;
                                if ((pensGetSI != null) && (get_IDCA_rj.id_cont_sintetic == 108)) sold_initial_CA = pensGetSI.sold_DP2016;
                            }

                        }



                        if (!nrcont_rj.Contains("."))
                        {

                            if (int.TryParse(nrcont_rj, out nrcontOK_rj) == false)
                            {
                                ViewData["Error"] = "nr de cont sintetic incorect";
                                return View("Error");
                            }

                            get_IDCS_rj = _context.conturi_sintetice.Where(gicarj => gicarj.nr_cont_sintetic == nrcontOK_rj).SingleOrDefault();

                            if (get_IDCS_rj == null)
                            {
                                ViewData["Error"] = "Nu exista acest nr de cont";
                                return View("Error");
                            }

                            if (get_IDCS_rj != null)
                                if (get_IDCS_rj.id_cont_sintetic < 100)
                                {
                                    ViewData["Error"] = "Acest tip de cont se selecteaza din lista";
                                    return View("Error");
                                }


                            if (get_IDCS_rj != null)
                                registru_jurnal = registru_jurnal
                                  .Where(rj => (rj.id_CS_debitor == get_IDCS_rj.id_cont_sintetic) || (rj.id_CS_creditor == get_IDCS_rj.id_cont_sintetic)).OrderBy(rj => rj.id_registru_jurnal);
                            get_si_rj = get_si_rj.Where(gsirj => (gsirj.id_CS_debitor == get_IDCS_rj.id_cont_sintetic) || (gsirj.id_CS_creditor == get_IDCS_rj.id_cont_sintetic)).OrderBy(rj => rj.id_registru_jurnal);

                            id_cont_CS_selectat = get_IDCS_rj.id_cont_sintetic;
                        }



                    }

            }

            //pa centr

            if (tip_selectie == 2)
            {
                if (id_cont_CA_selectat != 0)
                {
                    get_si_rj_sum = get_si_rj.Where(gsirj => gsirj.id_CA_creditor == id_cont_CA_selectat);
                    if (get_si_rj_sum.Count() > 0) total_credit_A = get_si_rj_sum.Sum(gsirj => gsirj.credit);

                    get_si_rj_sum = get_si_rj.Where(gsirj => gsirj.id_CA_debitor == id_cont_CA_selectat);
                    if (get_si_rj_sum.Count() > 0) total_debit_A = get_si_rj_sum.Sum(gsirj => gsirj.debit);
                }

                if (id_cont_CA_selectat == 0)
                    if (id_cont_CS_selectat != 0)
                    {
                        get_si_rj_sum = get_si_rj.Where(gsirj => gsirj.id_CS_creditor == id_cont_CS_selectat);
                        if (get_si_rj_sum.Count() > 0) total_credit_A = get_si_rj_sum.Sum(gsirj => gsirj.credit);

                        get_si_rj_sum = get_si_rj.Where(gsirj => gsirj.id_CS_debitor == id_cont_CS_selectat);
                        if (get_si_rj_sum.Count() > 0) total_debit_A = get_si_rj_sum.Sum(gsirj => gsirj.debit);
                    }
            }




            var get_tip_contA = await _context.conturi_analitice.Where(gtca => gtca.id_cont_analitic == id_cont_CA_selectat).SingleOrDefaultAsync();
            var get_tip_contS = await _context.conturi_sintetice.Where(gtcs => gtcs.id_cont_sintetic == id_cont_CS_selectat).SingleOrDefaultAsync();

            if (id_cont_CA_selectat == 0)
            {
                nr_cont = get_tip_contS.nr_cont_sintetic.ToString();
                activ = get_tip_contS.activ;
            }

            if (id_cont_CS_selectat == 0)
            {
                get_tip_contS = await _context.conturi_sintetice.Where(gtcs => gtcs.id_cont_sintetic == get_tip_contA.id_cont_sintetic).SingleOrDefaultAsync();
                activ = get_tip_contS.activ;
                nr_cont = get_tip_contA.nr_cont_analitic;
            }

            //CALCUL  SOLD INITIAL

            //SI anterior pt CS
            if (id_cont_CS_selectat == 101) SI_anterior = 31877454.73;
            if (id_cont_CS_selectat == 103) SI_anterior = 26975812.66;
            if (id_cont_CS_selectat == 109) SI_anterior = 5118013.46; // e cek
            if (id_cont_CS_selectat == 102) SI_anterior = 205283.50;
            if (id_cont_CS_selectat == 108) SI_anterior = 3495.81;
            if (id_cont_CS_selectat == 106) SI_anterior = 1080904.21;
            if (id_cont_CS_selectat == 107) SI_anterior = 493349.39;
            if (id_cont_CS_selectat == 18) SI_anterior = 57347.48;
            if (id_cont_CS_selectat == 52) SI_anterior = 5800325.56;
            if (id_cont_CS_selectat == 53) SI_anterior = 961252.58;
            if (id_cont_CS_selectat == 32) SI_anterior = -37111.52;
            //pa
            if (id_cont_CS_selectat == 4) SI_anterior = 205556.83;
                        
            if (id_cont_CS_selectat == 15) SI_anterior = 14350;
            if (id_cont_CS_selectat == 105) SI_anterior = 75883;
            
            //106,107,?108
            //52, 53, 54
            if (id_cont_CS_selectat == 51) SI_anterior = 53498.85;
            if (id_cont_CA_selectat == 101) SI_anterior = 53498.85;

            if (id_cont_CS_selectat == 26) SI_anterior = 233221.68;
            if (id_cont_CS_selectat == 27) SI_anterior = 1547.02;
            if (id_cont_CS_selectat == 18) SI_anterior = 57347.48;

            if (id_cont_CS_selectat == 53) SI_anterior = 4841994.95;


            if (activ)
                SI_rj = (decimal)SI_anterior + total_debit_A - total_credit_A;

            if (!activ)
                SI_rj = (decimal)SI_anterior + total_credit_A - total_debit_A;

            //CALCUL DEBIT si CREDIT luna curenta
            if (tip_selectie == 2)
            {
                if (id_cont_CA_selectat != 0)
                {
                    registru_jurnal_sum = registru_jurnal.Where(rj => rj.id_CA_creditor == id_cont_CA_selectat);
                    if (registru_jurnal_sum.Count() > 0) total_credit = registru_jurnal_sum.Sum(rj => rj.credit);

                    registru_jurnal_sum = registru_jurnal.Where(rj => rj.id_CA_debitor == id_cont_CA_selectat);
                    if (registru_jurnal_sum.Count() > 0) total_debit = registru_jurnal_sum.Sum(rj => rj.debit);
                }

                if (id_cont_CS_selectat != 0)
                {
                    registru_jurnal_sum = registru_jurnal.Where(rj => rj.id_CS_creditor == id_cont_CS_selectat);
                    if (registru_jurnal_sum.Count() > 0) total_credit = registru_jurnal_sum.Sum(rj => rj.credit);

                    registru_jurnal_sum = registru_jurnal.Where(rj => rj.id_CS_debitor == id_cont_CS_selectat);
                    if (registru_jurnal_sum.Count() > 0) total_debit = registru_jurnal_sum.Sum(rj => rj.debit);
                }
            }

            if (tip_selectie == 1)
                if (registru_jurnal.Count() > 0)
                {
                    total_credit = registru_jurnal.Sum(rj => rj.credit);
                    total_debit = registru_jurnal.Sum(rj => rj.debit);
                }

            //CALCUL SOLD FINAL
            if (activ)
                if (sold_initial_CA != 0) SI_rj = (decimal)SI_anterior + sold_initial_CA + (total_debit_A - total_credit_A);

            if (!activ)
                if (sold_initial_CA != 0) SI_rj = (decimal)SI_anterior + sold_initial_CA + (total_credit_A - total_debit_A);

            if (activ)
                SF_rj = SI_rj + total_debit - total_credit;

            if (!activ)
                SF_rj = SI_rj + total_credit - total_debit;



            // if (id_cont_CS_selectat==51) SF_rj = SI_rj - total_debit + total_credit;

            ViewData["SoldInitial_RJ"] = SI_rj.ToString("N2");
            ViewData["TotalDebit_RJ"] = total_debit.ToString("N2");
            ViewData["TotalCredit_RJ"] = total_credit.ToString("N2");
            ViewData["SoldFinal_RJ"] = SF_rj.ToString("N2");


            //pa_09_2024
            //ViewBag.TotalDebit_RJ = total_debit_A;
            // ViewBag.TotalCredit_RJ = total_credit_A;
            // ViewBag.SoldInitial_RJ = sold_curent;

            var ContSinteticList = _context.conturi_sintetice
                                          .Where(cs => cs.id_cont_sintetic < 100)
                                          .OrderBy(cs => cs.nr_cont_sintetic)
                                          .Select(cs =>
                          new SelectListItem
                          {


                              Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                              Value = cs.id_cont_sintetic.ToString()
                          });

            @ViewBag.id_cont_sintetic_rj = new SelectList(ContSinteticList, "Value", "Text");
          

           
            ViewData["luna_rj"] = luna_rj;
            ViewData["nrcont_rj"] = nrcont_rj;
            ViewData["id_sintetic_rj_id"] = id_cont_sintetic_rj;
            ViewData["id_analitic_rj_id"] = id_cont_analitic_rj;
            ViewData["tip_selectie"] = tip_selectie;


            if ((id_cont_sintetic_rj == 51) && (id_cont_analitic_rj == 101)) ViewBag.id_sintetic_rj_id = 101;
            if ((id_cont_sintetic_rj == 51) && (id_cont_analitic_rj == 151)) ViewBag.id_sintetic_rj_id = 151;
            if ((id_cont_sintetic_rj == 51) && (id_cont_analitic_rj == 152)) ViewBag.id_sintetic_rj_id = 152;
            if ((id_cont_sintetic_rj == 51) && (id_cont_analitic_rj == 153)) ViewBag.id_sintetic_rj_id = 153;
            if ((id_cont_sintetic_rj == 51) && (id_cont_analitic_rj == 154)) ViewBag.id_sintetic_rj_id = 154;
            if ((id_cont_sintetic_rj == 51) && (id_cont_analitic_rj == 155)) ViewBag.id_sintetic_rj_id = 155;
            if ((id_cont_sintetic_rj == 51) && (id_cont_analitic_rj == 156)) ViewBag.id_sintetic_rj_id = 156;
            //pa_09_2024

            if (lunaOK_rj == 0) TitluPerioadaRJ = " din ziua " + ziuaOK_rj.ToString("dd.MM.yyyy");
            if (lunaOK_rj != 0) TitluPerioadaRJ = " din luna " + lunaOK_rj.ToString();
            if (lunaOK_rj == 13) TitluPerioadaRJ = " din anul " + DateTime.Now.Year;



            ViewData["Titlu_rj"] = "Registru jurnal " + TitluPerioadaRJ;
            ViewData["Titlut_rj"] = "Total registru jurnal " + TitluPerioadaRJ;

            if (tip_selectie == 2)
            {

                ViewData["Titlu_rj"] = "Total fisa cont " + nr_cont + TitluPerioadaRJ;
                ViewData["Titlut_rj"] = "Fisa cont " + nr_cont + TitluPerioadaRJ;
            }

         


            if (tip_selectie != 1) registru_jurnal = registru_jurnal.OrderBy(rj => rj.data);


            //nr doc
          if (nr_document != "0" ) 
                registru_jurnal = registru_jurnal.Where(rjnrd => rjnrd.nr_document == nr_document.Trim()).OrderBy(rjnrd => rjnrd.data);
            ViewData["nr_document"] = nr_document;
            if (nr_document != "0")
            {
                total_debit = registru_jurnal.Sum(tdn => tdn.credit);
                total_credit = registru_jurnal.Sum(tdn => tdn.credit);
                ViewData["Titlu_rj"] = "Registru jurnal pentru nr. " + nr_document;
                ViewData["Titlut_rj"] = "Total registru jurnal pentru nr.  " + nr_document;
                ViewData["SoldInitial_RJ"] = "0.00";
                ViewData["TotalDebit_RJ"] = total_debit.ToString("N2");
                ViewData["TotalCredit_RJ"] = total_credit.ToString("N2");
                ViewData["SoldFinal_RJ"] = "0.00";

            }


            var pageNumber = page ?? 1;
            ViewBag.rj_select = registru_jurnal.ToList().ToPagedList(pageNumber, 12);
                       


            return View(ViewBag.rj_select);

            //return View(registru_jurnal.ToPagedList(pageNumber, pageSize));
        }
                  


        // GET: registru_jurnal/Create
        public IActionResult Create()
        {

            var ContSinteticList = _context.conturi_sintetice
                                        .Where(cs => cs.id_cont_sintetic < 100)
                                          .OrderBy(cs => cs.nr_cont_sintetic)
                                        .Select(cs =>
                        new SelectListItem
                        {

                            Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                            Value = cs.id_cont_sintetic.ToString()
                        });

            ViewData["ID_CS_debitor"] = new SelectList(ContSinteticList, "Value", "Text");

            var ContSinteticList_c = _context.conturi_sintetice
                                         .Where(cs => cs.id_cont_sintetic < 100)
                                         .OrderBy(cs => cs.nr_cont_sintetic).
                                         Select(cs =>
                             new SelectListItem
                             {

                                 Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                                 Value = cs.id_cont_sintetic.ToString()
                             });

            ViewData["ID_CS_creditor"] = new SelectList(ContSinteticList_c, "Value", "Text");

            return View();

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id_registru_jurnal,data,id_document,nr_document,id_CS_debitor,id_CA_debitor,id_CS_creditor,id_CA_creditor,explicatie_nr_cont_analitic,SI_CS_debit,SI_CA_debit,SI_CS_credit,SI_CA_credit,debit,credit,SF_CS_debit,SF_CA_debit,SF_CS_credit,SF_CA_credit,tip_document,sortare")] registru_jurnal registru_jurnal)
        {
         
            int ida;
            //29 dec

            int id_CS_RC = 0;
            int id_CA_RC = 0;
            decimal si_c = 0;
            decimal si_d = 0;
            decimal sf_c = 0;
            decimal sf_d = 0;
            string errormessage = "";
            DateTime maxData;


            if ((DateTime.Now - registru_jurnal.data).Days>40) 
            {
                ViewData["Error"] = "Nu se pot introduce articole contabile atat de vechi";
                return View("Error");
            }


            if ((registru_jurnal.data -  DateTime.Now).Days > 1)
            {
                ViewData["Error"] = "Nu se pot introduce articole contabile cu data mai mare ca data curenta";
                return View("Error");
            }


            if ( (int.TryParse(registru_jurnal.id_CS_debitor.ToString(), out ida) == false) || (registru_jurnal.id_CS_debitor==0) )
            {
                ViewData["Error"] = "Introduceti cont sintetic debitor";
                return View("Error");
            }

            if ((int.TryParse(registru_jurnal.id_CS_creditor.ToString(), out ida) == false) || (registru_jurnal.id_CS_creditor == 0))

            {
                ViewData["Error"] = "Introduceti cont sintetic creditor";
                return View("Error");
            }

            if (  (registru_jurnal.data - DateTime.Now).Days != 0)
            {

             maxData = _context.registru_jurnal.Where(rjmaxData => (rjmaxData.data.Day == registru_jurnal.data.Day)
                             && (rjmaxData.data.Month == registru_jurnal.data.Month) && (rjmaxData.data.Year == registru_jurnal.data.Year)).OrderByDescending(rjmaxDAta=>rjmaxDAta.data).First().data;
             maxData = maxData.AddMinutes(1);
             registru_jurnal.data = maxData;

            }
            

            registru_jurnal.nr_document = '_' + registru_jurnal.nr_document;
            registru_jurnal.sortare = "F" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();           
                    

            ModelState.Remove("conturi_sintetice_C");
            ModelState.Remove("conturi_sintetice_D");
            ModelState.Remove("conturi_analitice_C");
            ModelState.Remove("conturi_analitice_D");

                       
            if ((registru_jurnal.id_CS_debitor==18) || (registru_jurnal.id_CS_creditor == 18))
            {
            si_c = calcul_sold_initial(registru_jurnal.id_CS_creditor, registru_jurnal.id_CA_creditor);
            si_d = calcul_sold_initial(registru_jurnal.id_CS_debitor, registru_jurnal.id_CA_debitor);
            sf_c = calcul_sold_final2(registru_jurnal.id_CS_creditor, registru_jurnal.id_CA_creditor, registru_jurnal.credit, "C", si_c);
            sf_d = calcul_sold_final2(registru_jurnal.id_CS_debitor, registru_jurnal.id_CA_debitor, registru_jurnal.credit, "D", si_d);
            }

            registru_jurnal.debit = registru_jurnal.credit;
            registru_jurnal.SI_CA_debit = si_d;
            registru_jurnal.SI_CA_credit = si_c;
            registru_jurnal.SF_CA_debit = sf_d;
            registru_jurnal.SF_CA_credit = sf_c;

                                 
           

            if (ModelState.IsValid)
            {               

                _context.Add(registru_jurnal);       
                await _context.SaveChangesAsync();                

                return RedirectToAction("Index", "registru_jurnal", new { ziua_rj = DateTime.Now, luna_rj = "0", nrcont_rj = "0", id_cont_sintetic_rj = "0", id_cont_analitic_rj = "0", tip_selectie = 1, page = 1 });

            }


            var ContSinteticList = _context.conturi_sintetice
                                           .Where(cs => cs.id_cont_sintetic < 100)
                                             .OrderBy(cs => cs.nr_cont_sintetic)
                                           .Select(cs =>
                           new SelectListItem
                           {

                               Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                               Value = cs.id_cont_sintetic.ToString()
                           });

            ViewData["ID_CS_debitor"] = new SelectList(ContSinteticList, "Value", "Text");

            var ContSinteticList_c = _context.conturi_sintetice
                                         .Where(cs => cs.id_cont_sintetic < 100)
                                         .OrderBy(cs => cs.nr_cont_sintetic).
                                         Select(cs =>
                             new SelectListItem
                             {

                                 Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                                 Value = cs.id_cont_sintetic.ToString()
                             });

            ViewData["ID_CS_creditor"] = new SelectList(ContSinteticList_c, "Value", "Text");
                       
            

            return View(registru_jurnal);


        }


        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        // GET: registru_jurnal/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registru_jurnal = await _context.registru_jurnal.FindAsync(id);
            if (registru_jurnal == null)
            {
                return NotFound();
            }


            if ((registru_jurnal.id_CS_debitor > 100) || (registru_jurnal.id_CS_creditor > 100))
            {
                ViewData["Error"] = "Nu se pot face editari pentru acest tip de articol contabil";
                return View("Error");
            }


            if ((registru_jurnal.id_CS_debitor ==53) || (registru_jurnal.id_CS_creditor == 53))
            {
                ViewData["Error"] = "Nu se pot face editari pentru acest tip de articol contabil";
                return View("Error");
            }


            if ((registru_jurnal.id_CS_debitor == 27) || (registru_jurnal.id_CS_creditor == 27))
            {
                ViewData["Error"] = "Nu se pot face editari pentru acest tip de articol contabil";
                return View("Error");
            }


            //am pus
            var ContSinteticListE = _context.conturi_sintetice
                                       .Where(cs => cs.id_cont_sintetic < 100)
                                         .OrderBy(cs => cs.nr_cont_sintetic)
                                       .Select(cs =>
                       new SelectListItem
                       {

                           Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                           Value = cs.id_cont_sintetic.ToString()
                       });

            ViewData["ID_CS_debitor"] = new SelectList(ContSinteticListE, "Value", "Text");

            var ContSinteticList_cE = _context.conturi_sintetice
                                         .Where(cs => cs.id_cont_sintetic < 100)
                                         .OrderBy(cs => cs.nr_cont_sintetic).
                                         Select(cs =>
                             new SelectListItem
                             {

                                 Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                                 Value = cs.id_cont_sintetic.ToString()
                             });

            ViewData["ID_CS_creditor"] = new SelectList(ContSinteticList_cE, "Value", "Text");


            int idCSD = _context.registru_jurnal.Where(idcsd => (idcsd.id_registru_jurnal == id)).SingleOrDefault().id_CS_debitor;
            int idCSC = _context.registru_jurnal.Where(idcsd => (idcsd.id_registru_jurnal== id)).SingleOrDefault().id_CS_creditor;
            int idCAD = _context.registru_jurnal.Where(idcsd => (idcsd.id_registru_jurnal == id)).SingleOrDefault().id_CA_debitor;
            int idCAC = _context.registru_jurnal.Where(idcsd => (idcsd.id_registru_jurnal == id)).SingleOrDefault().id_CA_creditor;
            ViewData["ID_CS_debitor_S"] = idCSD;
            ViewData["ID_CS_creditor_S"] = idCSC;
            ViewData["ID_CA_debitor_S"] = idCAD.ToString();
            ViewData["ID_CA_creditor_S"] = idCAC.ToString();

            return View(registru_jurnal);
        }


        // POST: registru_jurnal/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id_registru_jurnal,data,id_document,nr_document,id_CS_debitor,id_CA_debitor,id_CS_creditor,id_CA_creditor,explicatie_nr_cont_analitic,SI_CS_debit,SI_CA_debit,SI_CS_credit,SI_CA_credit,debit,credit,SF_CS_debit,SF_CA_debit,SF_CS_credit,SF_CA_credit,tip_document,sortare")] registru_jurnal registru_jurnal)
        {

            //TEST iun 2025
            //face update above creditor 18 
            //face update above debitor 18



            int testDataRJedit = 0;

            testDataRJedit = 0;
            if ((registru_jurnal.data.Year) != (DateTime.Now.Year)) testDataRJedit = 1;
            if ((registru_jurnal.data.Month) != (DateTime.Now.Month)) testDataRJedit = 1;
            if ((registru_jurnal.data.Month) == (DateTime.Now.Month))
                if ((registru_jurnal.data.Day) != (DateTime.Now.Day))
                    testDataRJedit = 1;

            if ((registru_jurnal.id_CS_debitor != 51) || (registru_jurnal.id_CS_creditor != 51))
                testDataRJedit = 0;

            if (testDataRJedit == 1)
            {
                ViewData["Error"] = "Nu se pot face editari la articolele contabile decat pentru data curenta";
                return View("Error");
            }

            testDataRJedit = 0;         
            if ((registru_jurnal.id_CS_debitor != 51) || (registru_jurnal.id_CS_creditor != 51))
            if ((DateTime.Now - registru_jurnal.data).Days > 35)
               testDataRJedit = 1;
           
            
            if (testDataRJedit == 1)
            {
                ViewData["Error"] = "Nu se pot face editari la articolele contabile decat pentru luna curenta";
                return View("Error");
            }

            if ((registru_jurnal.id_CS_debitor == 0) || (registru_jurnal.id_CS_creditor == 0))
            {
                ViewData["Error"] = "Introduce-ti cont sintetic debitor sau creditor";
                return View("Error");
            }

            if ((registru_jurnal.id_CA_debitor == 0) || (registru_jurnal.id_CA_creditor == 0))
            {
                ViewData["Error"] = "Introduce-ti cont analitic debitor sau creditor";
                return View("Error");
            }

            if ((registru_jurnal.id_CS_debitor >100)  || (registru_jurnal.id_CS_creditor > 100))
            {
                ViewData["Error"] = "Nu se pot face editari pentru acest tip de articol contabil";
                return View("Error");
            }



            if ((registru_jurnal.id_CS_debitor == 27) || (registru_jurnal.id_CS_creditor ==27))
            {
                ViewData["Error"] = "Nu se pot face editari pentru acest tip de articol contabil";
                return View("Error");
            }

            if ((registru_jurnal.id_CS_debitor == 26) || (registru_jurnal.id_CS_creditor == 26))
            {
                ViewData["Error"] = "Nu se pot face editari pentru acest tip de articol contabil";
                return View("Error");
            }
                       


            if (id != registru_jurnal.id_registru_jurnal)
            {
                return NotFound();
            }

            ModelState.Remove("conturi_sintetice_D");
            ModelState.Remove("conturi_sintetice_C");
            ModelState.Remove("conturi_analitice_D");
            ModelState.Remove("conturi_analitice_C");

            registru_jurnal.debit = registru_jurnal.credit;


            var rjBeforeEdit = await _context.registru_jurnal.AsNoTracking()
              .Where(rjbe => rjbe.id_registru_jurnal == registru_jurnal.id_registru_jurnal).SingleOrDefaultAsync();


            var cekECD = await _context.registru_jurnal.Where(cedr => ((cedr.id_CA_debitor == registru_jurnal.id_CA_debitor) || (cedr.id_CA_creditor == registru_jurnal.id_CA_debitor)) && (cedr.data > registru_jurnal.data)).ToListAsync();
            var cekECC = await _context.registru_jurnal.Where(cedr => ((cedr.id_CA_debitor == registru_jurnal.id_CA_creditor) || (cedr.id_CA_creditor == registru_jurnal.id_CA_creditor)) && (cedr.data > registru_jurnal.data)).ToListAsync();
                  
                                
            if (ModelState.IsValid)
            {

                registru_jurnal.SI_CA_debit = calcul_sold_initial_E(registru_jurnal.id_CS_debitor, registru_jurnal.id_CA_debitor, registru_jurnal.data);
                registru_jurnal.SI_CA_credit = calcul_sold_initial_E(registru_jurnal.id_CS_creditor, registru_jurnal.id_CA_creditor, registru_jurnal.data);

                registru_jurnal.SF_CA_debit = calcul_sold_final2(registru_jurnal.id_CS_debitor, registru_jurnal.id_CA_debitor, registru_jurnal.credit, "D", registru_jurnal.SI_CA_debit);
                registru_jurnal.SF_CA_credit = calcul_sold_final2(registru_jurnal.id_CS_creditor, registru_jurnal.id_CA_creditor, registru_jurnal.credit, "C", registru_jurnal.SI_CA_credit);
                

                try
                {
                    _context.Update(registru_jurnal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!registru_jurnalExists(registru_jurnal.id_registru_jurnal))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                                               
                var upCBd = await _context.registru_jurnal.Where(cedr => (cedr.id_CS_debitor == 18)  && (cedr.data > registru_jurnal.data)).ToListAsync();
                decimal si_D = 0;
                foreach (registru_jurnal upcb in upCBd)
                {
                    si_D = 0;
                    si_D = calcul_sold_initial_E(upcb.id_CS_debitor, upcb.id_CA_debitor, upcb.data);
                    upcb.SF_CA_debit = calcul_sold_final2(upcb.id_CS_debitor, upcb.id_CA_debitor, upcb.credit, "D", si_D);
                    upcb.SI_CA_debit = si_D;
                    await _context.SaveChangesAsync();
                }

                //update cont casa si banca credit
                var upCBc = await _context.registru_jurnal.Where(cedr => (cedr.id_CS_creditor == 18) && (cedr.data > registru_jurnal.data)).ToListAsync();
                decimal si_c = 0;
                foreach (registru_jurnal upcb in upCBc)
                {
                    si_c = 0;
                    si_c = calcul_sold_initial_E(upcb.id_CS_creditor, upcb.id_CA_creditor, upcb.data);
                    upcb.SF_CA_credit = calcul_sold_final2(upcb.id_CS_creditor, upcb.id_CA_creditor, upcb.credit, "C", si_c);
                    upcb.SI_CA_credit = si_c;
                    await _context.SaveChangesAsync();
                }



                return RedirectToAction("Index", "registru_jurnal", new { ziua_rj = DateTime.Now, luna_rj = "0", nrcont_rj = "0", id_cont_sintetic_rj = "0", id_cont_analitic_rj = "0", tip_selectie = 1, page = 1 });
            }

            var ContSinteticListE = _context.conturi_sintetice
                                       .Where(cs => cs.id_cont_sintetic < 100)
                                         .OrderBy(cs => cs.nr_cont_sintetic)
                                       .Select(cs =>
                       new SelectListItem
                       {

                           Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                           Value = cs.id_cont_sintetic.ToString()
                       });

            ViewData["ID_CS_debitor"] = new SelectList(ContSinteticListE, "Value", "Text");

            var ContSinteticList_cE = _context.conturi_sintetice
                                         .Where(cs => cs.id_cont_sintetic < 100)
                                         .OrderBy(cs => cs.nr_cont_sintetic).
                                         Select(cs =>
                             new SelectListItem
                             {

                                 Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                                 Value = cs.id_cont_sintetic.ToString()
                             });

            ViewData["ID_CS_creditor"] = new SelectList(ContSinteticList_cE, "Value", "Text");


            return View(registru_jurnal);
        }

     

        private bool registru_jurnalExists(int id)
        {
            return _context.registru_jurnal.Any(e => e.id_registru_jurnal == id);
        }


        public ActionResult RC_delegati_afisare(string ziua_rcd = "01/01/2015", string ziua_rcd2 = "01/01/2015", int delegat = 0, int? page = 1)
        {
            DateTime ziua_rcd_OK;
            DateTime ziua_rcd2_OK;
            int ID_CA_D = 0;
            int iduser_rjd = 0;
            decimal RJDTotalIncasare = 0;
            decimal RJDTotalPlata = 0;
            decimal RJDTotalIncasAnterior = 0, RJDTotalPlatasAnterior = 0;
            decimal SI_RJD = 0;
            decimal SA_RJD = 0;
            string pdfrjd = "";
            string zona_delegat = "";
            int get_iduser_rjd=0;

            if (DateTime.TryParse(ziua_rcd, out ziua_rcd_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }



           




            get_iduser_rjd = 0;
            string userIdrjd = User.Identity.Name.Trim();
            var get_iduser_rjd_db = _context.utilizatoris
              .Where(u => u.nume_user == userIdrjd).SingleOrDefault();
            get_iduser_rjd = get_iduser_rjd_db.IDUser;

            if (get_iduser_rjd > 10) ziua_rcd2=ziua_rcd;


            if (DateTime.TryParse(ziua_rcd2, out ziua_rcd2_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }

            if (ziua_rcd_OK > ziua_rcd2_OK )
            { 
             ViewData["Error"] = "Ati introdus o data calendaristica de inceput mai mare";
             return View("Error");
            }
            
            if ( ((ziua_rcd2_OK - ziua_rcd_OK).Days) > 31)
            { 
              ViewData["Error"] = "Nu se poate scoate registrul de casa pentru mai multe luni";
              return View("Error");
            }

            ziua_rcd2_OK = ziua_rcd2_OK.AddHours(23);
            ziua_rcd2_OK = ziua_rcd2_OK.AddMinutes(59);
            ziua_rcd2_OK = ziua_rcd2_OK.AddSeconds(59);
            ViewData["ziua_rcd"] = ziua_rcd_OK.ToString("MM/dd/yyyy");
            ViewData["ziua_rcd2"] = ziua_rcd2_OK.ToString("MM/dd/yyyy");                    
           
            if (get_iduser_rjd ==16)  delegat = 1;
            if (get_iduser_rjd == 17) delegat = 3;
            if (get_iduser_rjd == 19) delegat = 4;
            if (get_iduser_rjd == 18) delegat = 2;
            if (get_iduser_rjd == 20) delegat = 5;
            if (get_iduser_rjd == 22) delegat = 7;
            ViewData["ShowListD"]=0;

           
            if (get_iduser_rjd <10) ViewData["ShowListD"] = 1;

            ID_CA_D = 101;

            if (delegat == 1)
            {
                ID_CA_D = 151;
                zona_delegat = "SAELELE";
            }

            if (delegat == 3)
            {
                ID_CA_D = 152;
                zona_delegat = "LUNCA";
            }

            if (delegat == 2)
            {
                ID_CA_D = 153;
                zona_delegat = "GIUVARASTI";
            }


            if (delegat == 4)
            {
                ID_CA_D = 154;
                zona_delegat = "TRAIAN";
            }


            if (delegat == 5)
            {
                ID_CA_D = 155;
                zona_delegat = "ISLAZ";
            }

            if (delegat == 7)
            {
                ID_CA_D = 156;
                zona_delegat = "LITA";
            }

            var rj_delegati = _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                   .Where(rjd => ((rjd.id_CA_debitor == ID_CA_D) || (rjd.id_CA_creditor == ID_CA_D)) && (rjd.data >= ziua_rcd_OK) && (rjd.data <= ziua_rcd2_OK)).OrderBy(rjd=>rjd.sortare).ThenBy(rjd => rjd.nr_document);


            if (rj_delegati.Count() > 0)
            {

                RJDTotalIncasare = 0;
                var rj_delegati_i = rj_delegati.Where(rjd => rjd.id_CA_debitor == ID_CA_D);
                if (rj_delegati_i.Count() > 0)
                    RJDTotalIncasare = rj_delegati_i.Sum(rjd => rjd.debit);


                RJDTotalPlata = 0;
                var rj_delegati_p = rj_delegati.Where(rjd => rjd.id_CA_creditor == ID_CA_D);
                if (rj_delegati_p.Count() > 0)
                    RJDTotalPlata = rj_delegati_p.Sum(rjd => rjd.credit);

            }

            //SI si SF calcul
            RJDTotalIncasAnterior = 0;
            var IncasAnteriorRJD = _context.registru_jurnal.Where(rjd => (rjd.id_CA_debitor == ID_CA_D) && (rjd.data < ziua_rcd_OK));                //        .Where(tiaa => ((tiaa.data < ziuaOK) && (tiaa.tip == "incasare") && (tiaa.data.Year == ziuaOK.Year))).Sum(tiaa => tiaa.incasare);
            if (IncasAnteriorRJD.Count() > 0) RJDTotalIncasAnterior = IncasAnteriorRJD.Sum(iarjd => iarjd.debit);

            RJDTotalPlatasAnterior = 0;
            var PlatasAnteriorRJD = _context.registru_jurnal.Where(rjd => (rjd.id_CA_creditor == ID_CA_D) && (rjd.data < ziua_rcd_OK));                //        .Where(tiaa => ((tiaa.data < ziuaOK) && (tiaa.tip == "incasare") && (tiaa.data.Year == ziuaOK.Year))).Sum(tiaa => tiaa.incasare);
            if (PlatasAnteriorRJD.Count() > 0) RJDTotalPlatasAnterior = PlatasAnteriorRJD.Sum(iarjd => iarjd.credit);

            SI_RJD = 0;
            if (ID_CA_D==101) SA_RJD = 53498.85M;
            SI_RJD = RJDTotalIncasAnterior - RJDTotalPlatasAnterior + SA_RJD;

            ViewData["SI_RJD"] = SI_RJD.ToString("N2");
            ViewData["RJDTotalIncasare"] = RJDTotalIncasare.ToString("N2");
            ViewData["RJDTotalPlata"] = RJDTotalPlata.ToString("N2");

            ViewData["SF_RCD"] = (SI_RJD + RJDTotalIncasare - RJDTotalPlata).ToString("N2");
            ViewData["delegat"] = delegat;
            ViewData["iduserd"] = get_iduser_rjd;

            ViewData["zi2init"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            var pageNumber = page ?? 1;
            ViewData["rc_delegati"] = rj_delegati.ToList().ToPagedList(pageNumber, 15);
                      

            string perioadaRCView = " din perioada " + ziua_rcd_OK.ToString("dd.MM.yyyy") + "-" + ziua_rcd2_OK.ToString("dd.MM.yyyy");
            if ((ziua_rcd2_OK - ziua_rcd_OK).Days == 0) perioadaRCView = " din ziua de " + ziua_rcd_OK.ToString("dd.MM.yyyy");

           if (delegat != 6)          
           {
                ViewData["TitlutRJD"] = "Total analitic incasari si plati CASIER " + zona_delegat + perioadaRCView;
                ViewData["TitluRJD"] = "Analitic incasari si plati CASIER " + zona_delegat + perioadaRCView;
            }

            if (delegat == 6)                 
            {
                ViewData["TitlutRJD"] = "Total analitic incasari si plati " + perioadaRCView;
                ViewData["TitluRJD"] = "Analitic incasari si plati " + perioadaRCView;
            }

            if (ziua_rcd_OK.Year == 2015)
            {
                ViewData["TitlutRJD"] = "Total analitic incasari si plati " + perioadaRCView;
                ViewData["TitluRJD"] = "Analitic incasari si plati " + perioadaRCView;
            }
            return View(ViewData["rc_delegati"]);

        }
        
        public async Task<IActionResult> RC_listare(DateTime ziua1_rc_l, DateTime ziua2_rc_l , int id_delegat_l, decimal SI_RC, decimal SF_RC, decimal incast, decimal platat)
        {


            if ( (ziua2_rc_l - ziua1_rc_l).Days != 0)
                if (id_delegat_l==6) 
                {
                ViewData["Error"] = "Nu se pot face decat listari zilnice pentru registru de casa sediu";
                return View("Error");
                }



            //pb
            // 2 date

            //ok cek code mai tb practic

            //2 record incas 2 plata - all coresp col rj
            //2 zile cu old si,sf,incas,plata

            int ID_CA_l = 0;           
            string zona_delegat_l = "";

            string get_nrCS="";
            string get_nrCA = "";
            string get_explCA = "";
            decimal sil = 0,sfl=0, incasl=0, platal=0;
            int i = 0;


            ziua2_rc_l = ziua2_rc_l.AddHours(23);
            ziua2_rc_l = ziua2_rc_l.AddMinutes(59);

            //if dif mai mare ca 30 

            ID_CA_l = 0;
           
            if (id_delegat_l == 1) id_delegat_l = 16;
            if (id_delegat_l == 2) id_delegat_l = 18;
            if (id_delegat_l == 3) id_delegat_l = 17;
            if (id_delegat_l == 4) id_delegat_l = 19;
            if (id_delegat_l == 5) id_delegat_l = 20;
            if (id_delegat_l == 7) id_delegat_l = 22;




            if (id_delegat_l == 16)
            {
                ID_CA_l = 151;
                zona_delegat_l = "SAELELE";
            }

            if (id_delegat_l == 17)
            {
                ID_CA_l = 152;
                zona_delegat_l = "LUNCA";
            }

            if (id_delegat_l == 18)
            {
                ID_CA_l = 153;
                zona_delegat_l = "GIUVARASTI";
            }

            if (id_delegat_l == 19)
            {
                ID_CA_l = 154;
                zona_delegat_l = "TRAIAN";
            }


            if (id_delegat_l == 20)
            {
                ID_CA_l = 155;
                zona_delegat_l = "ISLAZ";
            }

            if (id_delegat_l == 22)
            {
                ID_CA_l = 156;
                zona_delegat_l = "LITA";
            }

            if (id_delegat_l == 6) ID_CA_l = 101;

            var rc_listare =await _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                   //  .Where(rjd => ((rjd.id_CA_debitor == ID_CA_l) || (rjd.id_CA_creditor == ID_CA_l)) && (rjd.data >= ziua1_rc_l) && (rjd.data <= ziua2_rc_l)).OrderBy(rjd => rjd.sortare).ThenBy(rjd => rjd.nr_document).ToListAsync();
                   .Where(rjd => ((rjd.id_CA_debitor == ID_CA_l) || (rjd.id_CA_creditor == ID_CA_l)) && (rjd.data >= ziua1_rc_l) && (rjd.data <= ziua2_rc_l)).OrderBy(rjd => rjd.data).ToListAsync();

            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4, false);
            writer.SetCloseStream(false);                     

            document.SetMargins(100f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(13);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13).AddStyle(normal);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(13)
                .AddStyle(normal);

           // var table = new Table(7, true);

            //var table=new Table(new float[] { 1.2F,1, 1,1,2,1,1,1,1 }, true);
            var table = new Table(new float[] {1.5F, 1, 1, 1, 2, 1, 1 }, true);

            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(12);

            Style normalTbl2 = new Style();
            PdfFont fontTbl2 = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl2.SetFont(fontTbl).SetFontSize(9);


            Cell nrd_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph("nr document")).SetBold().AddStyle(normalTbl);

            Cell data_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                     .Add(new Paragraph("data")).SetBold().AddStyle(normalTbl);

            Cell nrcs_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph("nr. cont sintetic")).AddStyle(normalTbl).SetBold();


            Cell nrca_titlu = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("nr. cont analitic")).SetBold().AddStyle(normalTbl);

            Cell eca_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                     .Add(new Paragraph("explicatie cont analitic")).AddStyle(normalTbl).SetBold();          


            Cell incas_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                     .Add(new Paragraph("incasare")).AddStyle(normalTbl).SetBold();


            Cell plata_titlu = new Cell(1, 1)
                 .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("plata")).AddStyle(normalTbl).SetBold();
                      

            table.AddHeaderCell(nrd_titlu);
            table.AddHeaderCell(data_titlu);
            table.AddHeaderCell(nrcs_titlu);
            table.AddHeaderCell(nrca_titlu);
            table.AddHeaderCell(eca_titlu);            
            table.AddHeaderCell(incas_titlu);
            table.AddHeaderCell(plata_titlu);
           

           

            foreach (registru_jurnal rcl in rc_listare)
            {

                Cell nrd = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                         .Add(new Paragraph(rcl.nr_document).AddStyle(normalTbl2));
                table.AddCell(nrd);

                Cell data = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                      .Add(new Paragraph(rcl.data.ToString("dd.MM.yyyy")).AddStyle(normalTbl2));
                table.AddCell(data);

               
                get_nrCS = "";
                if (rcl.id_CS_creditor == 51) get_nrCS = rcl.conturi_sintetice_D.nr_cont_sintetic.ToString();
                if (rcl.id_CS_debitor == 51) get_nrCS = rcl.conturi_sintetice_C.nr_cont_sintetic.ToString();
                Cell nrcs = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                      .Add(new Paragraph(get_nrCS).AddStyle(normalTbl2));
                table.AddCell(nrcs);


                get_nrCA = "";
                get_explCA = "";
                sil = 0;
                sfl = 0;
                incasl = 0;
                platal = 0;
                if (rcl.id_CS_creditor == 51)
                { 
                    get_nrCA = rcl.conturi_analitice_D.nr_cont_analitic;
                    get_explCA = rcl.conturi_analitice_D.explicatie_nr_cont_analitic;                  
                    platal = rcl.debit;
                }
                if (rcl.id_CS_debitor == 51)
                {
                    get_nrCA = rcl.conturi_analitice_C.nr_cont_analitic;
                    get_explCA = rcl.conturi_analitice_C.explicatie_nr_cont_analitic;                   
                    incasl = rcl.credit;
                }
                Cell nrca = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                         .Add(new Paragraph(get_nrCA).AddStyle(normalTbl2));
                table.AddCell(nrca);

                Cell eca = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                       .Add(new Paragraph(get_explCA).AddStyle(normalTbl2));
                table.AddCell(eca);

                //pa cek code

                
                Cell incas = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                       .Add(new Paragraph(incasl.ToString("N2")).AddStyle(normalTbl2));
                table.AddCell(incas);


                Cell plata = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                      .Add(new Paragraph(platal.ToString("N2")).AddStyle(normalTbl2));
                table.AddCell(plata);

               

            }


            Paragraph ptsi = new Paragraph();
            ptsi.Add(new Text("Sold intial ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptsi.Add(new Text(SI_RC.ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));

            Paragraph ptcd = new Paragraph();
            ptcd.Add(new Text("Total incasari ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptcd.Add(new Text(incast.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));
            ptcd.Add(new Text("Total plati ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal).SetBold());
            ptcd.Add(new Text(platat.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal));

            Paragraph ptsf = new Paragraph();
            ptsf.Add(new Text("Sold final ").SetTextAlignment(TextAlignment.LEFT)
           .SetFontSize(13).AddStyle(normal).SetBold());
            ptsf.Add(new Text(SF_RC.ToString
                ("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));


            document.Add(ptsi);
            document.Add(ptcd);
            document.Add(ptsf);

            document.Add(new Paragraph(" "));

            document.Add(table);
            table.Complete();

            i = 1;
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y - 20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }


            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float y = 20f;
                float x = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "                                                                                                                       " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                //document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("30.04.2025 15:36") + "                                                                                                                       " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);

            }

            string perioadaRCl = " din perioada " + ziua1_rc_l.ToString("dd.MM.yyyy") + "-" + ziua2_rc_l.ToString("dd.MM.yyyy");
            if ((ziua2_rc_l - ziua1_rc_l).Days == 0) perioadaRCl = " din ziua de " + ziua1_rc_l.ToString("dd.MM.yyyy");

            if (id_delegat_l==6)
            titlu1_raport_rj = "Registru de casa " + perioadaRCl;

            if (id_delegat_l != 6)
                titlu1_raport_rj = "Registru de casa CASIER " + zona_delegat_l + perioadaRCl;                   

            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

            document.Close();


            document.Close();
            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");

            return fileStreamResult;


        }

        public ActionResult RCC_delegati(string ziua_rccd = "01/01/2015", string ziua2_rccd = "01/01/2015", int delegat = 0, string SI_RCC = "0")
        {
            //ok am cek all cod teoretic

            int ID_CA_D_RCC = 0;
            string zona_delegat_RCC = "";
            DateTime ziua_rccd_OK;
            DateTime ziua2_rccd_OK;
            string pdfrccd = "";
            string expl_CSC_deleg = "";
            int nr_CSC_deleg = 0;
            string expl_CSD_deleg = "";
            string nr_CSD_deleg = "";
            decimal total_credit_RCC = 0;
            decimal total_debit_RCC = 0;
            decimal SF_RCC = 0;

            if (DateTime.TryParse(ziua_rccd, out ziua_rccd_OK) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }

            if (DateTime.TryParse(ziua2_rccd, out ziua2_rccd_OK) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }

            ziua2_rccd_OK = ziua2_rccd_OK.AddHours(23);
            ziua2_rccd_OK = ziua2_rccd_OK.AddMinutes(59);

            ID_CA_D_RCC = 101;

            if (delegat == 1)
            {
                ID_CA_D_RCC = 151;
                zona_delegat_RCC = "SAELELE";
                expl_CSD_deleg = "SAELELE";
                nr_CSD_deleg = "5311.2";
            }

            if (delegat == 2)
            {
                ID_CA_D_RCC = 153;
                zona_delegat_RCC = "GIUVARASTI";
                expl_CSD_deleg = "GIUVARASTI";
                nr_CSD_deleg = "5311.5";
            }

            if (delegat == 3)
            {
                ID_CA_D_RCC = 152;
                zona_delegat_RCC = "LUNCA";
                expl_CSD_deleg = "LUNCA";
                nr_CSD_deleg = "5311.3";
            }

            if (delegat == 4)
            {
                ID_CA_D_RCC = 154;
                zona_delegat_RCC = "TRAIAN";
                expl_CSD_deleg = "TRAIAN";
                nr_CSD_deleg = "5311.4";
            }

            if (delegat == 5)
            {
                ID_CA_D_RCC = 155;
                zona_delegat_RCC = "ISLAZ";
                expl_CSD_deleg = "ISLAZ";
                nr_CSD_deleg = "5311.6";
            }

            if (delegat == 7)
            {
                ID_CA_D_RCC = 156;
                zona_delegat_RCC = "LITA";
                expl_CSD_deleg = "LITA";
                nr_CSD_deleg = "5311.7";
            }

            var get_info_CS_deleg = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == 1).SingleOrDefault();

            var rcc = _context.registru_jurnal.Where(rccd => (rccd.id_CA_debitor == ID_CA_D_RCC) && (rccd.data >= ziua_rccd_OK) && (rccd.data <= ziua2_rccd_OK))
           .GroupBy(crc => new
           {
               crc.id_CS_creditor,
           })
                 .Select(g => new
                 {
                     id_CS_D = ID_CA_D_RCC,
                     id_CS_C = g.Key.id_CS_creditor,
                     total_credit = g.Sum(i => i.credit),
                     total_debit = 0,
                     sortare = "H"
                 }
                 ).ToList();

            var rcc_d = _context.registru_jurnal.Where(rccd => (rccd.id_CA_creditor == ID_CA_D_RCC) && (rccd.data >= ziua_rccd_OK) && (rccd.data <= ziua2_rccd_OK))
            .GroupBy(crc => new
            {
                crc.id_CS_debitor,
            })
                  .Select(g => new
                  {
                      id_CS_D = ID_CA_D_RCC,
                      id_CS_C = g.Key.id_CS_debitor,
                      total_debit = g.Sum(i => i.debit),
                      total_credit = 0,
                      sortare = "H"
                  }
                  ).ToList();


            var RCC_list = new List<registru_casa_centralizator>();
            int id_rccd=0;

            var cekIncasCSplata = RCC_list.Where(rccdl => (rccdl.nrcont_CS == "700" )).SingleOrDefault();




            foreach (var rccd in rcc)
            {
                get_info_CS_deleg = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == rccd.id_CS_C).SingleOrDefault();
                expl_CSC_deleg = get_info_CS_deleg.explicatie_nr_cont_sintetic;
                nr_CSC_deleg = get_info_CS_deleg.nr_cont_sintetic;

                id_rccd=id_rccd + 1;

                RCC_list.Add(new registru_casa_centralizator
                {
                    idrccd=id_rccd ,
                    nrcont_CS = nr_CSC_deleg.ToString(),
                    explicatie_cont_CS = expl_CSC_deleg,                     
                    incasare = rccd.total_credit,
                    plata = 0,
                    sortare = rccd.sortare

                });

                _context.SaveChanges();

                //list  pdfrccd += ....
            }

            foreach (var rccc in rcc_d)
            {
                get_info_CS_deleg = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == rccc.id_CS_C).SingleOrDefault();
                expl_CSC_deleg = get_info_CS_deleg.explicatie_nr_cont_sintetic;
                nr_CSC_deleg = get_info_CS_deleg.nr_cont_sintetic;

                id_rccd = id_rccd + 1;


                // 
                cekIncasCSplata = RCC_list.Where(rccdl => (rccdl.nrcont_CS== nr_CSC_deleg.ToString())).SingleOrDefault();
                if (cekIncasCSplata != null) 
                    cekIncasCSplata.plata=rccc.total_debit;
              
                if (cekIncasCSplata == null)
                RCC_list.Add(new registru_casa_centralizator
                {
                    idrccd = id_rccd,
                    nrcont_CS = nr_CSC_deleg.ToString(),
                    explicatie_cont_CS = expl_CSC_deleg,                   
                    plata = rccc.total_debit,
                    incasare = 0,
                    sortare = rccc.sortare

                });

                _context.SaveChanges();              
               
            }    

           
            total_credit_RCC = 0;
            total_debit_RCC = 0;           
            total_credit_RCC = RCC_list.Sum(rccdl => rccdl.incasare);          
            total_debit_RCC = RCC_list.Sum(rccdl => rccdl.plata);
            SF_RCC = decimal.Parse(SI_RCC) + total_credit_RCC - total_debit_RCC;                 

           
            ViewData["SI_RCC_deleg"] = SI_RCC;
           
            ViewData["Total_Credit_RCC_deleg"] = total_credit_RCC.ToString("N2");
            
            ViewData["Total_Debit_RCC_deleg"] = total_debit_RCC.ToString("N2");
           
            ViewData["SF_RCC_deleg"]= SF_RCC.ToString("N2");

            string perioadaRCCView = " din perioada " + ziua_rccd_OK.ToString("dd.MM.yyyy") + "-" + ziua2_rccd_OK.ToString("dd.MM.yyyy");
            if ((ziua_rccd_OK - ziua2_rccd_OK).Days == 0) perioadaRCCView = " din ziua de " + ziua_rccd_OK.ToString("dd.MM.yyyy");

            if (delegat != 6)
            {
                ViewData["TitlutRCCD"] = "Total sintetic incasari si plati CASIER " + zona_delegat_RCC + perioadaRCCView;
                ViewData["TitluRCCD"] = "Sintetic incasari si plati CASIER " + zona_delegat_RCC + perioadaRCCView;
            }

            if (delegat == 6)
            {
                ViewData["TitlutRCCD"] = "Total sintetic incasari si plati  " + perioadaRCCView;
                ViewData["TitluRCCD"] = "Sintetic incasari si plati " + perioadaRCCView;
            }


            ViewData["ziua_RCCD"] = ziua_rccd_OK;
            ViewData["ziua2_RCCD"] = ziua2_rccd_OK;
            ViewData["delegat_RCCD"] = delegat;
            //pa cek code
            var RCC_list_ordonat = RCC_list.OrderBy(rccl => rccl.nrcont_CS);
            return View(RCC_list_ordonat);
        }

        public async Task<IActionResult> RCC_listare(string ziua_rccd = "01/01/2015", string ziua2_rccd = "01/01/2015", int delegat = 0, string SI_RCC = "0")
        {       

            int ID_CA_D_RCC = 0;
            string zona_delegat_RCC = "";
            DateTime ziua_rccd_OK;
            DateTime ziua2_rccd_OK;
            string pdfrccd = "";
            string expl_CSC_deleg = "";
            int nr_CSC_deleg = 0;
            string expl_CSD_deleg = "";
            string nr_CSD_deleg = "";
            decimal total_credit_RCC = 0;
            decimal total_debit_RCC = 0;
            decimal SF_RCC = 0;

            if (DateTime.TryParse(ziua_rccd, out ziua_rccd_OK) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }

            if (DateTime.TryParse(ziua2_rccd, out ziua2_rccd_OK) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }

            if (delegat==6)
            if ((ziua2_rccd_OK-ziua_rccd_OK).Days != 0 ) 
            {
                ViewData["Error"] = "Nu se pot face decat listari zilnice pentru registru de casa sediu";
                return View("Error");
            }

            //  ziua2_rccd_OK = ziua2_rccd_OK.AddHours(23);
            //ziua2_rccd_OK = ziua2_rccd_OK.AddMinutes(59);

            ID_CA_D_RCC = 101;


            if (delegat == 1)
            {
                ID_CA_D_RCC = 151;
                zona_delegat_RCC = "SAELELE";
                expl_CSD_deleg = "SAELELE";
                nr_CSD_deleg = "5311.2";
            }

            if (delegat == 3)
            {
                ID_CA_D_RCC = 152;
                zona_delegat_RCC = "LUNCA";
                expl_CSD_deleg = "LUNCA";
                nr_CSD_deleg = "5311.3";
            }

            if (delegat == 4)
            {
                ID_CA_D_RCC = 154;
                zona_delegat_RCC = "TRAIAN";
                expl_CSD_deleg = "TRAIAN";
                nr_CSD_deleg = "5311.4";
            }

            if (delegat == 2)
            {
                ID_CA_D_RCC = 153;
                zona_delegat_RCC = "GIUVARASTI";
                expl_CSD_deleg = "GIUVARASTI";
                nr_CSD_deleg = "5311.5";
            }

            if (delegat == 5)
            {
                ID_CA_D_RCC = 155;
                zona_delegat_RCC = "ISLAZ";
                expl_CSD_deleg = "ISLAZ";
                nr_CSD_deleg = "5311.6";
            }

            if (delegat == 7)
            {
                ID_CA_D_RCC = 156;
                zona_delegat_RCC = "LITA";
                expl_CSD_deleg = "LITA";
                nr_CSD_deleg = "5311.7";
            }

            var get_info_CS_deleg = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == 1).SingleOrDefault();

            var rcc = _context.registru_jurnal.Where(rccd => (rccd.id_CA_debitor == ID_CA_D_RCC) && (rccd.data >= ziua_rccd_OK) && (rccd.data <= ziua2_rccd_OK))
           .GroupBy(crc => new
           {
               crc.id_CS_creditor,
           })
                 .Select(g => new
                 {
                     id_CS_D = ID_CA_D_RCC,
                     id_CS_C = g.Key.id_CS_creditor,
                     total_credit = g.Sum(i => i.credit),
                     total_debit = 0,
                     sortare = "H"
                 }
                 ).ToList();

            var rcc_d = _context.registru_jurnal.Where(rccd => (rccd.id_CA_creditor == ID_CA_D_RCC) && (rccd.data >= ziua_rccd_OK) && (rccd.data <= ziua2_rccd_OK))
            .GroupBy(crc => new
            {
                crc.id_CS_debitor,
            })
                  .Select(g => new
                  {
                      id_CS_D = ID_CA_D_RCC,
                      id_CS_C = g.Key.id_CS_debitor,
                      total_debit = g.Sum(i => i.debit),
                      total_credit = 0,
                      sortare = "H"
                  }
                  ).ToList();


            var RCC_list = new List<registru_casa_centralizator>();
            
            int id_rccd = 0;

            var cekIncasCSplata = RCC_list.Where(rccdl => (rccdl.nrcont_CS == "700")).SingleOrDefault();


            foreach (var rccd in rcc)
            {
                get_info_CS_deleg = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == rccd.id_CS_C).SingleOrDefault();
                expl_CSC_deleg = get_info_CS_deleg.explicatie_nr_cont_sintetic;
                nr_CSC_deleg = get_info_CS_deleg.nr_cont_sintetic;

                id_rccd = id_rccd + 1;

                RCC_list.Add(new registru_casa_centralizator
                {
                    idrccd = id_rccd,
                    nrcont_CS = nr_CSC_deleg.ToString(),
                    explicatie_cont_CS = expl_CSC_deleg,
                    incasare = rccd.total_credit,
                    plata = 0,
                    sortare = rccd.sortare

                });

                _context.SaveChanges();
               
            }

            foreach (var rccc in rcc_d)
            {
                get_info_CS_deleg = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == rccc.id_CS_C).SingleOrDefault();
                expl_CSC_deleg = get_info_CS_deleg.explicatie_nr_cont_sintetic;
                nr_CSC_deleg = get_info_CS_deleg.nr_cont_sintetic;

                id_rccd = id_rccd + 1;
 
                cekIncasCSplata = RCC_list.Where(rccdl => (rccdl.nrcont_CS == nr_CSC_deleg.ToString())).SingleOrDefault();
                if (cekIncasCSplata != null)
                    cekIncasCSplata.plata = rccc.total_debit;

                if (cekIncasCSplata == null)
                    RCC_list.Add(new registru_casa_centralizator
                    {
                        idrccd = id_rccd,
                        nrcont_CS = nr_CSC_deleg.ToString(),
                        explicatie_cont_CS = expl_CSC_deleg,
                        plata = rccc.total_debit,
                        incasare = 0,
                        sortare = rccc.sortare

                    });

                _context.SaveChanges();

            }

            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4.Rotate(), false);
            writer.SetCloseStream(false);

            document.SetMargins(110f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(13);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13).AddStyle(normal);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(13)
                .AddStyle(normal);

            var table = new Table(4, true);
            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(12);


            Style normalTbl2 = new Style();
            PdfFont fontTbl2 = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl2.SetFont(fontTbl).SetFontSize(9);



            Cell CS_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("nr cont sintetic")).SetBold().AddStyle(normalTbl);

            Cell expl_CS_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("explicatie nr cont sintetic")).SetBold().AddStyle(normalTbl);


            Cell incasare_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("total incasare")).AddStyle(normalTbl).SetBold();

            Cell plata_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("total plata")).AddStyle(normalTbl).SetBold(); ;

            table.AddHeaderCell(CS_titlu);
            table.AddHeaderCell(expl_CS_titlu);
            table.AddHeaderCell(incasare_titlu);
            table.AddHeaderCell(plata_titlu);

            var RCC_list_ordonat = RCC_list.OrderBy(rccl => rccl.nrcont_CS);

            foreach (registru_casa_centralizator rcclp in RCC_list_ordonat)
            {

                Cell RCSnr_CSl = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph(rcclp.nrcont_CS).AddStyle(normalTbl2));
                table.AddCell(RCSnr_CSl);


                Cell RCSexpl_CSl = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                       .Add(new Paragraph(rcclp.explicatie_cont_CS).AddStyle(normalTbl2));
                table.AddCell(RCSexpl_CSl);


                Cell RCSincasl = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.RIGHT)
                       .Add(new Paragraph(rcclp.incasare.ToString("N2")).AddStyle(normalTbl2));
                table.AddCell(RCSincasl);

                Cell RCSplatal = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.RIGHT)
                       .Add(new Paragraph(rcclp.plata.ToString("N2")).AddStyle(normalTbl2));
                table.AddCell(RCSplatal);


            }


            total_credit_RCC = 0;
            total_debit_RCC = 0;
            total_credit_RCC = RCC_list.Sum(rccdl => rccdl.incasare);
            total_debit_RCC = RCC_list.Sum(rccdl => rccdl.plata);
            SF_RCC = decimal.Parse(SI_RCC) + total_credit_RCC - total_debit_RCC;


            Paragraph ptsi = new Paragraph();
            ptsi.Add(new Text("Sold intial ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(12).AddStyle(normal).SetBold());
            ptsi.Add(new Text(decimal.Parse(SI_RCC).ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(12).AddStyle(normal));
            Paragraph ptcd = new Paragraph();
           
            ptcd.Add(new Text("Total incasari ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(12).AddStyle(normal).SetBold());
            ptcd.Add(new Text(total_credit_RCC.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(12).AddStyle(normal));
            ptcd.Add(new Text("Total plati ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(12).AddStyle(normal).SetBold());
            ptcd.Add(new Text(total_debit_RCC.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(12).AddStyle(normal));

            Paragraph ptsf = new Paragraph();
            ptsf.Add(new Text("Sold final ").SetTextAlignment(TextAlignment.LEFT)
           .SetFontSize(12).AddStyle(normal).SetBold());
            ptsf.Add(new Text(SF_RCC.ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(12).AddStyle(normal));


            document.Add(ptsi);
            document.Add(ptcd);
            document.Add(ptsf);

            document.Add(new Paragraph(" "));

            document.Add(table);
            table.Complete();

            int i = 1;
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y - 20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }


            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float y = 20f;
                float x = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "                                                                                                                                                                                            " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                //document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("30.04.2025 15:36") + "                                                                                                                                                                                            " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }
            string perioada = " din perioada " + ziua_rccd_OK.ToString("dd.MM.yyyy") + "-" + ziua2_rccd_OK.ToString("dd.MM.yyyy");
            if ((ziua_rccd_OK - ziua2_rccd_OK).Days == 0) perioada = " din ziua de " + ziua_rccd_OK.ToString("dd.MM.yyyy");


            if (delegat != 6)
                titlu1_raport_rj = "Registru de casa centralizator CASIER " + zona_delegat_RCC + perioada;
            if (delegat == 6)
                titlu1_raport_rj = "Registru de casa centralizator " + perioada;

            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

            document.Close();                         
           
            //pa cek code

            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");

            return (fileStreamResult);
            
        }

        public JsonResult GetNrContAnalitic_rj(int id_cont_sinteticSel_rj)
        {

            var nrContAnaliticList_rj = _context.conturi_analitice
                                       .Where(c => c.id_cont_sintetic == id_cont_sinteticSel_rj)
                                       .OrderBy(c => c.nr_cont_analitic)
                                       .Select(ca =>
                           new SelectListItem
                           {
                               Text = ca.nr_cont_analitic.ToString() + " " + ca.explicatie_nr_cont_analitic.ToString(),
                               Value = ca.id_cont_analitic.ToString()
                           });

            return Json(new SelectList(nrContAnaliticList_rj, "Value", "Text"), new System.Text.Json.JsonSerializerOptions());

        }


        public ActionResult registru_jurnal_centralizator_CS(string ziua_rj_ccs = "01/01/2015", string luna_rj_ccs = "0", string nrcont_rj_ccs = "120", int id_cont_sintetic_rj_ccs = 0, string sold_initial_CS = "0")
        {
            //2024-10-tblete

            int lunaOK_rj_ccs = 0;
            DateTime ziuaOK_rj_ccs = Convert.ToDateTime("2024-1-1");
            int an_rj_ccs = 2025;
            int nrcontOK_rj_ccs = 0;
            int id_CS_selectat_ccs = 0;

            int nrcont_selectat = 0;
            string expl_cont_selectat = "-";
            decimal SI_anterior = 0;
            decimal TotalDebit_RJ_CCS = 0;
            decimal TotalCredit_RJ_CCS = 0;
            string titluPerioada = "";
            Boolean activ_c = false;
            string nr_CSD_afisare = "";
            string nr_CSC_afisare = "";
            string pdfrj = "";
            string nrcont_selectat_titlu = "";
            int i=0;


            //de aici pt centr

            lunaOK_rj_ccs = Int32.Parse(luna_rj_ccs);

            var registru_jurnal = _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                .Where(rj => rj.id_registru_jurnal == 10);

            var get_IDCA_rj = _context.conturi_analitice.Where(gicarj => gicarj.nr_cont_analitic == "421.1").SingleOrDefault();
            var get_IDCS_rj = _context.conturi_sintetice.Where(gicsrj => gicsrj.nr_cont_sintetic == 2).SingleOrDefault();

            if (lunaOK_rj_ccs == 0)
            {
                if (DateTime.TryParse(ziua_rj_ccs, out ziuaOK_rj_ccs) == false)
                {
                    ViewData["Error"] = "Introduceti o valoare calendaristica pentru ziua";
                    return View("Error");
                }

                registru_jurnal = _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                       .Where(rj => (rj.data.Day == ziuaOK_rj_ccs.Day) && (rj.data.Month == ziuaOK_rj_ccs.Month) && (rj.data.Year == ziuaOK_rj_ccs.Year)).OrderBy(rj => rj.id_registru_jurnal);

            }

            if (lunaOK_rj_ccs != 0)
            {
                if (lunaOK_rj_ccs != 13)
                    registru_jurnal = _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                    .Where(rj => (rj.data.Month == lunaOK_rj_ccs) && (rj.data.Year == an_rj_ccs)).OrderBy(rj => rj.id_registru_jurnal);

                if (lunaOK_rj_ccs == 13)
                    registru_jurnal = _context.registru_jurnal.Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D)
                    .Where(rj => (rj.data.Year == an_rj_ccs)).OrderBy(rj => rj.id_registru_jurnal);
            }

            if ((id_cont_sintetic_rj_ccs != 0) && (id_cont_sintetic_rj_ccs <= 100))
            {
                registru_jurnal = registru_jurnal
                          .Where(rj => (rj.id_CS_debitor == id_cont_sintetic_rj_ccs) || (rj.id_CS_creditor == id_cont_sintetic_rj_ccs)).OrderBy(rj => rj.id_registru_jurnal);
                id_CS_selectat_ccs = id_cont_sintetic_rj_ccs;
            }
                         
            if ((id_cont_sintetic_rj_ccs == 101) || (id_cont_sintetic_rj_ccs == 151) || (id_cont_sintetic_rj_ccs == 152) || (id_cont_sintetic_rj_ccs == 153) || (id_cont_sintetic_rj_ccs == 154) || (id_cont_sintetic_rj_ccs == 155) || (id_cont_sintetic_rj_ccs == 156))
            {

                registru_jurnal = registru_jurnal
                          .Where(rj => (rj.id_CA_debitor == id_cont_sintetic_rj_ccs) || (rj.id_CA_creditor == id_cont_sintetic_rj_ccs)).OrderBy(rj => rj.id_registru_jurnal);
                id_CS_selectat_ccs = id_cont_sintetic_rj_ccs;
            }


            if (id_cont_sintetic_rj_ccs == 0)
            {

                if (int.TryParse(nrcont_rj_ccs, out nrcontOK_rj_ccs) == false)
                {
                    ViewData["Error"] = "nr de cont sintetic incorect";
                    return View("Error");
                }

                get_IDCS_rj = _context.conturi_sintetice.Where(gicarj => gicarj.nr_cont_sintetic == nrcontOK_rj_ccs).SingleOrDefault();

                if (get_IDCS_rj == null)
                {
                    ViewData["Error"] = "Nu exista acest nr de cont";
                    return View("Error");
                }

                if (get_IDCS_rj != null)
                    if (get_IDCS_rj.id_cont_sintetic < 100)
                    {
                        ViewData["Error"] = "Acest tip de cont se selecteaza din lista";
                        return View("Error");
                    }

                if (get_IDCS_rj != null)
                    registru_jurnal = registru_jurnal
                                   .Where(rj => (rj.id_CS_debitor == get_IDCS_rj.id_cont_sintetic) || (rj.id_CS_creditor == get_IDCS_rj.id_cont_sintetic)).OrderBy(rj => rj.id_registru_jurnal);

                id_CS_selectat_ccs = get_IDCS_rj.id_cont_sintetic;

            }

            var registru_jurnal_centralizator_cs_list = new List<registru_jurnal_centralizator_CS>();

            string webRootPath = _env.WebRootPath.ToString() + "\\PDF\\";
            PdfWriter writer = new PdfWriter(webRootPath + "registru_jurnal_centralizator.pdf");
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4.Rotate(), false);

            document.SetMargins(100f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(14);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13).AddStyle(normal);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(13)
                .AddStyle(normal);

            var table = new Table(6, true);            
            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(12);

            Style normalTbl2 = new Style();
            PdfFont fontTbl2 = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl2.SetFont(fontTbl).SetFontSize(9);

            Cell CSD_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("cont debit")).SetBold().AddStyle(normalTbl);

            Cell expl_CSD_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("explicatie cont debit")).SetBold().AddStyle(normalTbl); 

            Cell CSC_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("cont credit")).AddStyle(normalTbl).SetBold();


            Cell expl_CSC_titlu = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("explicatie cont credit")).SetBold().AddStyle(normalTbl);

            Cell debit_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("debit")).AddStyle(normalTbl).SetBold();

            Cell credit_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("credit")).AddStyle(normalTbl).SetBold(); ;

            table.AddHeaderCell(CSD_titlu);
            table.AddHeaderCell(expl_CSD_titlu);
            table.AddHeaderCell(CSC_titlu);
            table.AddHeaderCell(expl_CSC_titlu);
            table.AddHeaderCell(debit_titlu);
            table.AddHeaderCell(credit_titlu);



            ViewData["ziua_rj_ccs"] = ziua_rj_ccs;
            ViewData["luna_rj_ccs"] = lunaOK_rj_ccs;
            ViewData["nrcont_rj_ccs"] = nrcontOK_rj_ccs;
            ViewData["id_sintetic_rj_ccs"] = id_cont_sintetic_rj_ccs;
            ViewData["id_analitic_rj_ccs"] = "0";

            if (id_cont_sintetic_rj_ccs > 100)
            {
                ViewData["id_sintetic_rj_ccs"]= "51";
                ViewData["id_analitic_rj_ccs"] = id_cont_sintetic_rj_ccs;
            }

            var get_info_CS = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == id_CS_selectat_ccs).SingleOrDefault();
            if (get_info_CS != null)
            {
                nrcont_selectat = get_info_CS.nr_cont_sintetic;
                expl_cont_selectat = get_info_CS.explicatie_nr_cont_sintetic;
                activ_c = get_info_CS.activ;
            }


            string expl_CSD = "";
            string expl_CSC = "";
            int nr_CSD = 0;
            int nr_CSC = 0;


            var registru_jurnal_ccs_D = registru_jurnal.Where(rjccsd => rjccsd.id_CS_debitor == id_CS_selectat_ccs)
            .GroupBy(crc => new
            {
                crc.id_CS_debitor,
                crc.id_CS_creditor,
            })
                  .Select(g => new
                  {
                      id_CS_D = g.Key.id_CS_debitor,
                      id_CS_C = g.Key.id_CS_creditor,
                      total_debit = g.Sum(i => i.debit),
                      total_credit = 0,
                      sortare = "H"
                  }
                  ).ToList();


            //de aici
            if ((id_cont_sintetic_rj_ccs == 101) || (id_cont_sintetic_rj_ccs == 151) || (id_cont_sintetic_rj_ccs == 152) || (id_cont_sintetic_rj_ccs == 153) || (id_cont_sintetic_rj_ccs == 154) || (id_cont_sintetic_rj_ccs == 155) || (id_cont_sintetic_rj_ccs == 156))
                registru_jurnal_ccs_D = registru_jurnal.Where(rjccsd => rjccsd.id_CA_debitor == id_CS_selectat_ccs)
            .GroupBy(crc => new
            {
                crc.id_CS_debitor,
                crc.id_CS_creditor,
            })
                  .Select(g => new
                  {
                      id_CS_D = g.Key.id_CS_debitor,
                      id_CS_C = g.Key.id_CS_creditor,
                      total_debit = g.Sum(i => i.debit),
                      total_credit = 0,
                      sortare = "H"
                  }
                  ).ToList();
            //pa

            foreach (var rjccs in registru_jurnal_ccs_D)
            {
                nr_CSD_afisare = "";
                nr_CSC_afisare = "";

                get_info_CS = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == rjccs.id_CS_C).SingleOrDefault();
                expl_CSC = get_info_CS.explicatie_nr_cont_sintetic;
                nr_CSC = get_info_CS.nr_cont_sintetic;

                nr_CSD_afisare = nrcont_selectat.ToString();
                if (rjccs.id_CS_D == 53)
                {
                    nr_CSD_afisare = "472.01";
                    expl_cont_selectat = "VENITURI IN AVANS DIN DOBANZI";

                }

                if (rjccs.id_CS_D == 54)
                {
                    nr_CSD_afisare = "734.1";
                    expl_cont_selectat = "VENITURI DIN DOBANZI IMPRUMUTURI";

                }

                nr_CSC_afisare = nr_CSC.ToString();
                if (rjccs.id_CS_C == 53)
                {
                    nr_CSC_afisare = "472.01";
                    expl_CSC = "VENITURI IN AVANS DIN DOBANZI";

                }

                if (rjccs.id_CS_C == 54)
                {
                    nr_CSC_afisare = "734.1";
                    expl_CSC = "VENITURI DIN DOBANZI IMPRUMUTURI";

                }



                if ((id_cont_sintetic_rj_ccs == 101) || (id_cont_sintetic_rj_ccs == 151) || (id_cont_sintetic_rj_ccs == 152) || (id_cont_sintetic_rj_ccs == 153) || (id_cont_sintetic_rj_ccs == 154) || (id_cont_sintetic_rj_ccs == 155) || (id_cont_sintetic_rj_ccs == 156))
                {
                    nr_CSD_afisare = _context.conturi_analitice.Where(carjc => carjc.id_cont_analitic == id_CS_selectat_ccs).First().nr_cont_analitic;
                    expl_cont_selectat = _context.conturi_analitice.Where(carjc => carjc.id_cont_analitic == id_CS_selectat_ccs).First().explicatie_nr_cont_analitic;
                    activ_c = true;

                //    if (id_CS_selectat_ccs == 101)
                 //   {
                   //     nr_CSD_afisare = "5311.1";
                  //      expl_cont_selectat = "SEDIU";
                 //   }
                }

                registru_jurnal_centralizator_cs_list.Add(new registru_jurnal_centralizator_CS
                {
                    nrcont_CS_D = nr_CSD_afisare,
                    explicatie_cont_CS_D = expl_cont_selectat,
                    nrcont_CS_C = nr_CSC_afisare,
                    explicatie_cont_CS_C = expl_CSC,
                    total_credit = 0,
                    total_debit = rjccs.total_debit,
                    sortare = rjccs.sortare

                });

                _context.SaveChanges();

                Cell nr_CSDl= new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                         .Add(new Paragraph(nr_CSD_afisare).AddStyle(normalTbl2));                
                table.AddCell(nr_CSDl);

                Cell expl_CSDl = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph(expl_cont_selectat).AddStyle(normalTbl2));
                table.AddCell(expl_CSDl);

                Cell nr_CSCl = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                         .Add(new Paragraph(nr_CSC_afisare).AddStyle(normalTbl2));
                table.AddCell(nr_CSCl);

                Cell expl_CSCl = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph(expl_CSC).AddStyle(normalTbl2));
                table.AddCell(expl_CSCl);

                Cell total_Dl = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph(rjccs.total_debit.ToString("N2")).AddStyle(normalTbl2));
                table.AddCell(total_Dl);

                Cell total_Cl = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                       .Add(new Paragraph("0.00").AddStyle(normalTbl2));
                table.AddCell(total_Cl);
             

            }


            var registru_jurnal_ccs_C = registru_jurnal.Where(rjccsd => rjccsd.id_CS_creditor == id_CS_selectat_ccs)
           .GroupBy(crc => new
           {
               crc.id_CS_creditor,
               crc.id_CS_debitor,
           })
                 .Select(g => new
                 {
                     id_CS_D = g.Key.id_CS_debitor,
                     id_CS_C = g.Key.id_CS_creditor,
                     total_debit = 0,
                     total_credit = g.Sum(i => i.credit),
                     sortare = "H"
                 }
                 ).ToList();


            //de aici
            if ((id_cont_sintetic_rj_ccs == 101) || (id_cont_sintetic_rj_ccs == 151) || (id_cont_sintetic_rj_ccs == 152) || (id_cont_sintetic_rj_ccs == 153) || (id_cont_sintetic_rj_ccs == 154) || (id_cont_sintetic_rj_ccs == 155) || (id_cont_sintetic_rj_ccs == 156))
                registru_jurnal_ccs_C = registru_jurnal.Where(rjccsd => rjccsd.id_CA_creditor == id_CS_selectat_ccs)
             .GroupBy(crc => new
             {
                 crc.id_CS_creditor,
                 crc.id_CS_debitor,
             })
                   .Select(g => new
                   {
                       id_CS_D = g.Key.id_CS_debitor,
                       id_CS_C = g.Key.id_CS_creditor,
                       total_debit = 0,
                       total_credit = g.Sum(i => i.credit),
                       sortare = "H"
                   }
                ).ToList();
            //pa



            foreach (var rjccs in registru_jurnal_ccs_C)
            {
                nr_CSD_afisare = "";
                nr_CSC_afisare = "";

                get_info_CS = _context.conturi_sintetice.Where(gics => gics.id_cont_sintetic == rjccs.id_CS_D).SingleOrDefault();
                if (get_info_CS != null)
                {
                    expl_CSD = get_info_CS.explicatie_nr_cont_sintetic;
                    nr_CSD = get_info_CS.nr_cont_sintetic;
                }

                nr_CSD_afisare = nr_CSD.ToString();
                if (rjccs.id_CS_D == 53)
                {
                    nr_CSD_afisare = "472.01";
                    expl_CSD = "VENITURI IN AVANS DIN DOBANZI";

                }
                if (rjccs.id_CS_D == 54)
                {
                    nr_CSD_afisare = "734.1";
                    expl_CSD = "VENITURI DIN DOBANZI IMPRUMUTURI";
                }

                nr_CSC_afisare = nrcont_selectat.ToString();
                if (rjccs.id_CS_C == 53)
                {
                    nr_CSC_afisare = "472.01";
                    expl_cont_selectat = "VENITURI IN AVANS DIN DOBANZI";

                }

                if (rjccs.id_CS_C == 54)
                {
                    nr_CSC_afisare = "734.1";
                    expl_cont_selectat = "VENITURI DIN DOBANZI IMPRUMUTURI";
                }



                if ((id_cont_sintetic_rj_ccs == 101) || (id_cont_sintetic_rj_ccs == 151) || (id_cont_sintetic_rj_ccs == 152) || (id_cont_sintetic_rj_ccs == 153) || (id_cont_sintetic_rj_ccs == 154) || (id_cont_sintetic_rj_ccs == 155) || (id_cont_sintetic_rj_ccs == 156))
                {
                    nr_CSC_afisare = _context.conturi_analitice.Where(carjc => carjc.id_cont_analitic == id_CS_selectat_ccs).First().nr_cont_analitic;
                    expl_cont_selectat = _context.conturi_analitice.Where(carjc => carjc.id_cont_analitic == id_CS_selectat_ccs).First().explicatie_nr_cont_analitic;

                    if (id_CS_selectat_ccs == 101)
                    {
                        nr_CSC_afisare = "5311";
                        expl_cont_selectat = "SEDIU";
                    }

                }


                registru_jurnal_centralizator_cs_list.Add(new registru_jurnal_centralizator_CS
                {
                    nrcont_CS_D = nr_CSD_afisare,
                    explicatie_cont_CS_D = expl_CSD,
                    nrcont_CS_C = nr_CSC_afisare,
                    explicatie_cont_CS_C = expl_cont_selectat,
                    total_debit = 0,
                    total_credit = rjccs.total_credit,
                    sortare = rjccs.sortare

                });

                _context.SaveChanges();


                Cell nr_CSDcl = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                         .Add(new Paragraph(nr_CSD_afisare).AddStyle(normalTbl2));
                table.AddCell(nr_CSDcl);

                Cell expl_CSDcl = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph(expl_CSD).AddStyle(normalTbl2));
                table.AddCell(expl_CSDcl);

                Cell nr_CSCcl = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                         .Add(new Paragraph(nr_CSC_afisare).AddStyle(normalTbl2));
                table.AddCell(nr_CSCcl);

                Cell expl_CSCcl = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph(expl_cont_selectat).AddStyle(normalTbl2));
                table.AddCell(expl_CSCcl);

                Cell total_Dcl = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph("0.00").AddStyle(normalTbl2));
                table.AddCell(total_Dcl);

                Cell total_Ccl = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                       .Add(new Paragraph(rjccs.total_credit.ToString("N2")).AddStyle(normalTbl2));
                table.AddCell(total_Ccl);              

            }

            TotalDebit_RJ_CCS = registru_jurnal_centralizator_cs_list.Sum(rjccs => rjccs.total_debit);
            TotalCredit_RJ_CCS = registru_jurnal_centralizator_cs_list.Sum(rjccs => rjccs.total_credit);
            ViewData["TotalDebit_RJ_CCS"] = TotalDebit_RJ_CCS.ToString("N2");
            ViewData["TotalCredit_RJ_CCS"] = TotalCredit_RJ_CCS.ToString("N2");
            ViewData["SoldInitial_RJ_CCS"] = sold_initial_CS;

            if (activ_c)
                ViewData["SoldFinal_RJ_CCS"] = (decimal.Parse(sold_initial_CS) + TotalDebit_RJ_CCS - TotalCredit_RJ_CCS).ToString("N2");


            if (!activ_c)
                ViewData["SoldFinal_RJ_CCS"] = (decimal.Parse(sold_initial_CS) + TotalCredit_RJ_CCS - TotalDebit_RJ_CCS).ToString("N2");

            // @ViewBag.SoldInitial_RJ_CCS= TotalDebit_RJ_CCS.ToString("N");

            var ContSinteticList = _context.conturi_sintetice
                                          .Where(cs => cs.id_cont_sintetic < 100)
                                          .OrderBy(cs => cs.id_cont_sintetic)
                                          .Select(cs =>
                          new SelectListItem
                          {
                              Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                              Value = cs.id_cont_sintetic.ToString()
                          });

            ViewData["id_cont_sintetic_rj_ccs"] = new SelectList(ContSinteticList, "Value", "Text");


            if (lunaOK_rj_ccs != 0) titluPerioada = " din luna " + lunaOK_rj_ccs;
            if (lunaOK_rj_ccs == 0) titluPerioada = " din ziua " + ziuaOK_rj_ccs.ToString("dd.MM.yyyy");


            nrcont_selectat_titlu = nrcont_selectat.ToString();
            if (nrcont_selectat == 472) nrcont_selectat_titlu = "472.01";
            if (nrcont_selectat == 734) nrcont_selectat_titlu = "734.1";

            if (id_CS_selectat_ccs == 101) nrcont_selectat_titlu = "5311";
            if ((id_CS_selectat_ccs == 101) && (nrcont_rj_ccs == "113")) nrcont_selectat_titlu = "113"; ;
            if (id_CS_selectat_ccs == 151) nrcont_selectat_titlu = "5311.2";
            if (id_CS_selectat_ccs == 152) nrcont_selectat_titlu = "5311.3";
            if (id_CS_selectat_ccs == 153) nrcont_selectat_titlu = "5311.5";
            if (id_CS_selectat_ccs == 154) nrcont_selectat_titlu = "5311.4";
            if (id_CS_selectat_ccs == 155) nrcont_selectat_titlu = "5311.6";
            if (id_CS_selectat_ccs == 156) nrcont_selectat_titlu = "5311.7";

            ViewData["Titlu_rj_ccs"] = "Sintetic fisa cont " + nrcont_selectat_titlu + titluPerioada;
            ViewData["Titlut_rj_ccs"] = "Total sintetic fisa cont " + nrcont_selectat_titlu + titluPerioada;


            //pa ian 10
            //list

            Paragraph ptsi = new Paragraph();
            ptsi.Add(new Text("Sold intial ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptsi.Add(new Text(sold_initial_CS).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));
            Paragraph ptcd = new Paragraph();
            ptcd.Add(new Text("Total debit ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptcd.Add(new Text(TotalDebit_RJ_CCS.ToString("N2") + " ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));
            ptcd.Add(new Text("Total credit ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal).SetBold());
            ptcd.Add(new Text(TotalCredit_RJ_CCS.ToString("N2") + " ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal));
            Paragraph ptsf = new Paragraph();
            ptsf.Add(new Text("Sold final ").SetTextAlignment(TextAlignment.LEFT)
           .SetFontSize(13).AddStyle(normal).SetBold());
            ptsf.Add(new Text(@ViewBag.SoldFinal_RJ_CCS).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));
                      

            document.Add(ptsi);
            document.Add(ptcd);
            document.Add(ptsf);

            document.Add(new Paragraph(" "));

            document.Add(table);
            table.Complete();

            i = 1;
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y - 20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }


            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float y = 20f;
                float x = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy") + "                                                                                                                                                                            " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);

            }

            titlu1_raport_rj = "Sintetic fisa cont " + nrcont_selectat_titlu +  titluPerioada;
            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);
           

            document.Close();

           

            return View(registru_jurnal_centralizator_cs_list);
        }


        private class TableHeaderEventHandler : IEventHandler
        {
            private Table table;
            private float tableHeight;
            private Document doc;



            public TableHeaderEventHandler(Document doc)
            {
                this.doc = doc;
                InitTable();

                TableRenderer renderer = (TableRenderer)table.CreateRendererSubTree();
                renderer.SetParent(new DocumentRenderer(doc));

                // Simulate the positioning of the renderer to find out how much space the header table will occupy.
                LayoutResult result = renderer.Layout(new LayoutContext(new LayoutArea(0, PageSize.A4)));
                tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();
            }

            public void HandleEvent(Event currentEvent)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
                PageSize pageSize = pdfDoc.GetDefaultPageSize();
                float coordX = pageSize.GetX() + doc.GetLeftMargin();
                float coordY = pageSize.GetTop() - doc.GetTopMargin();
                float width = pageSize.GetWidth() - doc.GetRightMargin() - doc.GetLeftMargin();
                float height = GetTableHeight();
                Rectangle rect = new Rectangle(coordX, coordY, width, height);

                new Canvas(canvas, rect)
                    .Add(table)
                    .Close();
            }

            public float GetTableHeight()
            {
                return tableHeight;
            }

            private void InitTable()
            {
                Style normalh = new Style();
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                normalh.SetFont(font).SetFontSize(14);


                Cell h3 = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                         .Add(new Paragraph(titlu1_raport_rj).SetFontSize(13).SetBold().AddStyle(normalh));
                h3.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
              

                Cell h4 = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("").SetFontSize(13).SetBold().AddStyle(normalh));
                h4.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);

                table = new Table(1).UseAllAvailableWidth();
                table.AddCell(h3);
                table.AddCell(h4);
               
            }
        }



        public decimal calcul_sold_initial(int idcs, int idca)
        {

            var activ = _context.conturi_sintetice
               .Where(a => a.id_cont_sintetic == idcs).SingleOrDefault();

            decimal creditSI = 0, debitSI = 0;
            decimal SI_anterior = 0;

            SI_anterior = 0;

            var creditSI_db = _context.registru_jurnal.Where(csi => (csi.data < DateTime.Now) && (csi.id_CA_creditor == idca) && (csi.id_CS_creditor == idcs));
            if (creditSI_db.Count() >= 1)
                creditSI = creditSI_db.Sum(csi => csi.credit);
            var debitSI_db = _context.registru_jurnal.Where(dsi => (dsi.data < DateTime.Now) && (dsi.id_CA_debitor == idca) && (dsi.id_CS_debitor == idcs));
            if (debitSI_db.Count() >= 1)
                debitSI = debitSI_db.Sum(dsi => dsi.debit);

            if (idcs == 52) SI_anterior = 5800325.56M; 
            if (idcs == 51) SI_anterior = 53498.85M;

            var getnrca = _context.conturi_analitice.Where(g => g.id_cont_analitic == idca).SingleOrDefault();
            string nrcont4621 = "";
            if (idcs == 18)
            {
            nrcont4621 = getnrca.nr_cont_analitic;
            string getnrcarnet4621= "";
            getnrcarnet4621 =nrcont4621.Remove(0, 5);
            int nrcarnet4621 = 0;
            nrcarnet4621 = Int32.Parse(getnrcarnet4621);
            var pensGetSI4621 = _context.Pensionars.Where(pgsi4 => pgsi4.nrcarnet == nrcarnet4621).SingleOrDefault();
            if (pensGetSI4621.sold_462_2016!=1) SI_anterior = pensGetSI4621.sold_462_2016;
            }

            var getSI_calc_SI = _context.solduri_initiale.Where(gsi => (gsi.id_CA_SI == idca) && (gsi.an==DateTime.Now.Year-1)).SingleOrDefault();
            if (idcs==15)            
            if (getSI_calc_SI != null) SI_anterior = getSI_calc_SI.SI;
            // if (getSI_calc_SI == null) SI_anterior = 0;

            if (!activ.activ)
                return (SI_anterior + creditSI - debitSI);
            if (activ.activ)
                return (SI_anterior + debitSI - creditSI);

            return 0;
        }


        public decimal calcul_sold_final2(int idcs_sf, int idca_sf, decimal sum_sf, string tip_sf, decimal sold_i)
        {
            var activ_S = _context.conturi_sintetice
               .Where(a_s => a_s.id_cont_sintetic == idcs_sf).SingleOrDefault();

            if (tip_sf == "C")
            {
                if (activ_S.activ) return sold_i - sum_sf;
                if (!activ_S.activ) return sold_i + sum_sf;
            }


            if (tip_sf == "D")
            {
                if (activ_S.activ) return sold_i + sum_sf;
                if (!activ_S.activ) return sold_i - sum_sf;
            }


            return 0;
        }

        public decimal calcul_sold_initial_E(int idcs, int idca, DateTime datasi)
        {

            var activ = _context.conturi_sintetice
               .Where(a => a.id_cont_sintetic == idcs).SingleOrDefault();

            decimal creditSI = 0, debitSI = 0;
            decimal SI_anterior = 0;

            SI_anterior = 0;

            var creditSI_db = _context.registru_jurnal.Where(csi => (csi.data < datasi) && (csi.id_CA_creditor == idca) && (csi.id_CS_creditor == idcs));
            if (creditSI_db.Count() >= 1)
                creditSI = creditSI_db.Sum(csi => csi.credit);
            var debitSI_db = _context.registru_jurnal.Where(dsi => (dsi.data < datasi ) && (dsi.id_CA_debitor == idca) && (dsi.id_CS_debitor == idcs));
            if (debitSI_db.Count() >= 1)
                debitSI = debitSI_db.Sum(dsi => dsi.debit);

            if (idcs == 52) SI_anterior = 5800325.56M;
            if (idcs == 51) SI_anterior = 53498.85M;

            var getnrca = _context.conturi_analitice.Where(g => g.id_cont_analitic == idca).SingleOrDefault();
               
                     

            if (!activ.activ)
                return (SI_anterior + creditSI - debitSI);
            if (activ.activ)
                return (SI_anterior + debitSI - creditSI);

            return 0;
        }


        public async Task<IActionResult> tezaur(string ziua_T = "01/01/2015")
        {
            decimal get_IncasA_t = 0;
            decimal get_PlataA_t = 0;
            decimal SI_T = 0;
            decimal get_Incas_t = 0;
            decimal get_Plata_t = 0;
            decimal SF_T = 0;
            DateTime ziua_T_OK;
            string pdft = "";
            string pdfth = "";
            decimal SI_T_total = 0;
            decimal SF_T_total = 0;
            decimal incas_total = 0;
            decimal plata_total = 0;

            string nrcontTL = "";
            String explcontTL = "";

            if (DateTime.TryParse(ziua_T, out ziua_T_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }

            var CA_Tezaur =await _context.conturi_analitice.Where(cat => cat.id_cont_sintetic == 51).OrderBy(cat => cat.nr_cont_analitic).ToListAsync();

            var rj_t =await _context.registru_jurnal.Where(rjt => rjt.id_CS_debitor == 2).ToListAsync();

            SI_T = get_IncasA_t - get_PlataA_t;

            var tezaur_list = new List<tezaur>();

            int i = 0;

            //aici nume coloane tabel         



            string webRootPath = _env.WebRootPath.ToString() + "\\PDF\\";
            PdfWriter writer = new PdfWriter(webRootPath + "tezaur.pdf");
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4.Rotate(), false);

            document.SetMargins(100f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(13);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13).AddStyle(normal);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(13)
                .AddStyle(normal);

            var table = new Table(6, true);
            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(12);

            Style normalTbl2 = new Style();
            PdfFont fontTbl2 = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl2.SetFont(fontTbl).SetFontSize(9);

            Cell nrct_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("nr cont casa")).SetBold().AddStyle(normalTbl);

            Cell explct_titlu = new Cell(1, 1)
             .SetTextAlignment(TextAlignment.CENTER)
              .Add(new Paragraph("explicatie cont casa")).SetBold().AddStyle(normalTbl);

            Cell sit_titlu = new Cell(1, 1)
             .SetTextAlignment(TextAlignment.RIGHT)
              .Add(new Paragraph("sold initial")).SetBold().AddStyle(normalTbl);

            Cell incast_titlu = new Cell(1, 1)
             .SetTextAlignment(TextAlignment.RIGHT)
              .Add(new Paragraph("incasare")).SetBold().AddStyle(normalTbl);

            Cell platat_titlu = new Cell(1, 1)
              .SetTextAlignment(TextAlignment.RIGHT)
               .Add(new Paragraph("plata")).SetBold().AddStyle(normalTbl);

            Cell sft_titlu = new Cell(1, 1)
              .SetTextAlignment(TextAlignment.RIGHT)
               .Add(new Paragraph("sold final")).SetBold().AddStyle(normalTbl);

            table.AddHeaderCell(nrct_titlu);
            table.AddHeaderCell(explct_titlu);
            table.AddHeaderCell(sit_titlu);
            table.AddHeaderCell(incast_titlu);
            table.AddHeaderCell(platat_titlu);
            table.AddHeaderCell(sft_titlu);


            if (ziua_T_OK.Year >= 2025)
                foreach (conturi_analitice cat in CA_Tezaur)
                {

                    i = i + 1;

                    get_IncasA_t = 0;
                    get_PlataA_t = 0;
                    get_Incas_t = 0;
                    get_Plata_t = 0;
                    SI_T = 0;
                    SF_T = 0;

                   // if ((cat.id_cont_analitic != 153) || (cat.id_cont_analitic != 155))
                 //   {

                        rj_t =await _context.registru_jurnal.Where(git => (git.data < ziua_T_OK) && (git.id_CA_debitor == cat.id_cont_analitic)).ToListAsync();
                        if (rj_t.Count() >= 1)
                            get_IncasA_t = rj_t.Sum(git => git.credit);
                        rj_t =await  _context.registru_jurnal.Where(git => (git.data < ziua_T_OK) && (git.id_CA_creditor == cat.id_cont_analitic)).ToListAsync();
                        if (rj_t.Count() >= 1)
                            get_PlataA_t = rj_t.Sum(git => git.debit);
                        SI_T = get_IncasA_t - get_PlataA_t;

                        rj_t =await _context.registru_jurnal.Where(git => (git.data.Month == ziua_T_OK.Month) && (git.data.Day == ziua_T_OK.Day) && (git.data.Year == ziua_T_OK.Year) && (git.id_CA_debitor == cat.id_cont_analitic)).ToListAsync();
                        if (rj_t.Count() >= 1)
                            get_Incas_t = rj_t.Sum(git => git.credit);
                        rj_t =await _context.registru_jurnal.Where(git => (git.data.Month == ziua_T_OK.Month) && (git.data.Day == ziua_T_OK.Day) && (git.data.Year == ziua_T_OK.Year) && (git.id_CA_creditor == cat.id_cont_analitic)).ToListAsync();
                        if (rj_t.Count() >= 1)
                            get_Plata_t = rj_t.Sum(git => git.credit);

                        if (cat.id_cont_analitic == 101) SI_T = SI_T + 53498.85M;

                        
                        SF_T = SI_T + get_Incas_t - get_Plata_t;

                    string nrcc = "";
                    string explcc= "";

                    nrcc= "5311.1";
                    explcc = "SEDIU";

                    // if (cat.id_cont_analitic != 101) nrcc = cat.nr_cont_analitic;
                    //if (cat.id_cont_analitic != 101) explcc = cat.explicatie_nr_cont_analitic;

                    nrcc = cat.nr_cont_analitic;
                    explcc = cat.explicatie_nr_cont_analitic;

                    tezaur_list.Add(new tezaur
                        {
                            id_tezaur = i,
                            nr_cont_casa = nrcc,
                            explicatie_cont_casa = explcc,
                            SI_T = SI_T,
                            incas_T = get_Incas_t,
                            plata_T = get_Plata_t,
                            SF_T = SF_T,

                        });

                        _context.SaveChangesAsync();

                        nrcontTL = cat.nr_cont_analitic;
                        explcontTL = cat.explicatie_nr_cont_analitic;

                        if (cat.nr_cont_analitic == "-") nrcontTL = "5311";
                        if (cat.nr_cont_analitic == "-") explcontTL = "SEDIU";

                        Cell nrct = new Cell(1, 1)
                         .SetTextAlignment(TextAlignment.CENTER)
                           .Add(new Paragraph(nrcc)).AddStyle(normalTbl2);

                       Cell explct = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                         .Add(new Paragraph(explcc)).AddStyle(normalTbl2);

                       Cell sit = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(SI_T.ToString("N2"))).AddStyle(normalTbl2);

                       Cell incast = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(get_Incas_t.ToString("N2"))).AddStyle(normalTbl2);

                       Cell platat = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(get_Plata_t.ToString("N2"))).AddStyle(normalTbl2);

                       Cell sft = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .Add(new Paragraph(SF_T.ToString("N2"))).AddStyle(normalTbl2);

                   
                    table.AddCell(nrct);
                    table.AddCell(explct);
                    table.AddCell(sit);
                    table.AddCell(incast);
                    table.AddCell(platat);
                    table.AddCell(sft);

                }

            SI_T_total = tezaur_list.Sum(sitt => sitt.SI_T);
            SF_T_total = tezaur_list.Sum(sftt => sftt.SF_T);
            incas_total = tezaur_list.Sum(itt => itt.incas_T);
            plata_total = tezaur_list.Sum(itt => itt.plata_T);



            Paragraph ptsi = new Paragraph();
            ptsi.Add(new Text("Sold intial ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptsi.Add(new Text(SI_T_total.ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));

            Paragraph ptcd = new Paragraph();
            ptcd.Add(new Text("Total incasari ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptcd.Add(new Text(incas_total.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));
            ptcd.Add(new Text("Total plati ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal).SetBold());
            ptcd.Add(new Text(plata_total.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal));

            Paragraph ptsf = new Paragraph();
            ptsf.Add(new Text("Sold final ").SetTextAlignment(TextAlignment.LEFT)
           .SetFontSize(13).AddStyle(normal).SetBold());
            ptsf.Add(new Text(SF_T_total.ToString
                ("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));


            document.Add(ptsi);
            document.Add(ptcd);
            document.Add(ptsf);

            document.Add(new Paragraph(" "));

            document.Add(table);
            table.Complete();

            i = 1;
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y - 20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }


            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float y = 20f;
                float x = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy") + "                                                                                                                                                                                           " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);

            }


            titlu1_raport_rj = "Situatie tezaur din data de " + ziua_T_OK.ToString("dd.MM.yyyy");

            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

            document.Close();


            ViewData["sold_initialT"] = SI_T_total.ToString("N2");
            ViewData["sold_finalT"] = SF_T_total.ToString("N2");
            ViewData["RCTotalIncasareT"] = incas_total.ToString("N2");
            ViewData["RCTotalPlataT"] = plata_total.ToString("N2");
                       

            ViewData["Titlutt"] = "Total situatie tezaur din data de " + ziua_T_OK.ToString("MM/dd/yyyy");
            ViewData["Titlut"] = "Situatie tezaur din data de " + ziua_T_OK.ToString("MM/dd/yyyy");


            return View(tezaur_list);
        }

        public async Task<IActionResult> listare_fisa_cont(string ziua_rjll="01/01/2015" , int luna_rjl = 0, int tip_selectiel = 1, int id_cont_analitic_rjl = 0, int id_cont_sintetic_rjl = 0, string nr_document= "0", decimal silfcp=0, string nrcontl= "0")
        {
            //declaratii var

            DateTime ziua_rjl = DateTime.Parse(ziua_rjll);
            string nrcontD;
            string explContD;
            string nrcontC;
            string explContC;
            var getCS = await _context.conturi_sintetice.Where(gcs => gcs.id_cont_sintetic == id_cont_sintetic_rjl).SingleOrDefaultAsync();
            var rj_5121 =await _context.registru_jurnal.Where(rjb => rjb.id_CS_debitor == 1 ).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.id_document).ThenBy(rjb => rjb.id_CS_creditor).ToListAsync() ;
            var CAListFC = await _context.conturi_analitice.Where(calfc => calfc.nr_cont_analitic == "01").SingleOrDefaultAsync();
            var rjListFC = await _context.registru_jurnal.Where(rjlfc => rjlfc.id_CA_debitor == 1).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D).Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).ToListAsync();
            string SFSI18 = "";
            decimal soldCD = 0;
            Boolean activlfc ;

            decimal silfc = 0;
            silfc = silfcp;
            decimal SFlfc = 0;
            decimal platatlfc = 0;
            decimal incastlfc = 0;
            int testCDA = 0;
            string explCA= "";

            string colhh4P = "";
            string colhh5P = "";
            string colhh6P = "";
            string colhh7P = "";
            //init document

            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4, false);
            writer.SetCloseStream(false);

            document.SetMargins(100f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(13);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT).SetBold()
                .SetFontSize(12).AddStyle(normal);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT).SetBold()
               .SetFontSize(12)
                .AddStyle(normal);

            Paragraph blankl = new Paragraph("")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(12)
                .AddStyle(normal);



            string titlu = "0";
            string perioadal = "0";
            string nrcontT ="0";
            if (id_cont_analitic_rjl != 0)
            {
                CAListFC = await _context.conturi_analitice.Where(cat => cat.id_cont_analitic == id_cont_analitic_rjl).SingleOrDefaultAsync();
                nrcontT = CAListFC.nr_cont_analitic;
            }

           
            if (luna_rjl == 0) perioadal = " din ziua " + ziua_rjl.ToString("dd.MM.yyyy");
           
            if (luna_rjl != 0) perioadal = " din luna " + luna_rjl;
            if (nr_document != "0") titlu = "Nr. document " + nr_document + perioadal;
            if (luna_rjl == 13) perioadal = " din anul " + DateTime.Now.Year.ToString();
            if (nr_document == "0") titlu = "Operatii nr cont " + getCS.nr_cont_sintetic + perioadal;
            if ((nr_document == "0") && (id_cont_analitic_rjl!=0)) titlu = "Operatii nr cont " + nrcontT + perioadal;

           

            if ((nr_document != "0") &&
              ((nr_document == "2") || (nr_document == "3") || (nr_document == "5") || (nr_document == "4") || (nr_document == "7") || (nr_document == "10"))) titlu = "Nota contabila nr. " + nr_document + perioadal;


            Paragraph header3 = new Paragraph(titlu)
              .SetTextAlignment(TextAlignment.CENTER).SetBold()
              .SetFontSize(12)
               .AddStyle(normal);
                      

            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(12);

            Style normalTbl2 = new Style();
            PdfFont fontTbl2 = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl2.SetFont(fontTbl2).SetFontSize(9);

            Style normalTblHead = new Style();
            PdfFont fontTblHead = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTblHead.SetFont(fontTblHead).SetFontSize(11).SetBorder(Border.NO_BORDER);

            Style normalTblHead2 = new Style();
            PdfFont fontTblHead2 = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTblHead2.SetFont(fontTblHead2).SetFontSize(11).SetBold();
            //end init document

            testCDA = 0;
            if ( (id_cont_analitic_rjl==0) && ( (id_cont_sintetic_rjl==26) || (id_cont_sintetic_rjl == 27) || (id_cont_sintetic_rjl == 18)))
             testCDA = 1;

            // var table = new Table(9, true);
            int nrcoll = 0;
            nrcoll = 8;
          
            if (testCDA==0) nrcoll = 8;
            //wwif (nr_document != "0") nrcoll = 4;
            if (nr_document != "0") nrcoll = 8;
            var table = new Table(nrcoll, true);


            //wwif ((testCDA==0) && (nr_document == "0"))
            if (testCDA==0)  
            {

              activlfc = getCS.activ;

             if (nr_document == "0")
             {

             if (nrcontl.Contains("."))
             {
              CAListFC = _context.conturi_analitice.Where(gicarjl => gicarjl.nr_cont_analitic == nrcontl).SingleOrDefault();
              if (CAListFC != null) id_cont_sintetic_rjl = CAListFC.id_cont_sintetic;
              if (CAListFC != null) id_cont_analitic_rjl = CAListFC.id_cont_analitic;
              if (CAListFC != null) titlu = "Operatii nr cont " + CAListFC.nr_cont_analitic + perioadal;

              getCS = _context.conturi_sintetice.Where(gcslfc => gcslfc.id_cont_sintetic == CAListFC.id_cont_sintetic).SingleOrDefault();
              activlfc=getCS.activ;
             }
             
             if ((luna_rjl == 0) && (id_cont_analitic_rjl==0))
             rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Day == ziua_rjl.Day) && (rjb.data.Month == ziua_rjl.Month) && (rjb.data.Year == DateTime.Now.Year) && ((rjb.id_CS_debitor == id_cont_sintetic_rjl) || (rjb.id_CS_creditor == id_cont_sintetic_rjl))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();
             if ((luna_rjl != 0) && (id_cont_analitic_rjl == 0))
             rj_5121 =await _context.registru_jurnal.Where(rjb => (rjb.data.Month==luna_rjl) && (rjb.data.Year==DateTime.Now.Year) && ((rjb.id_CS_debitor == id_cont_sintetic_rjl) || (rjb.id_CS_creditor == id_cont_sintetic_rjl))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();
             if ((luna_rjl == 13) && (id_cont_analitic_rjl == 0))
             rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Year == DateTime.Now.Year) && ((rjb.id_CS_debitor == id_cont_sintetic_rjl) || (rjb.id_CS_creditor == id_cont_sintetic_rjl))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();


             if ((luna_rjl == 0) && (id_cont_analitic_rjl!=0))
             rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Day == ziua_rjl.Day) && (rjb.data.Month == ziua_rjl.Month) && (rjb.data.Year == DateTime.Now.Year) && ((rjb.id_CA_debitor == id_cont_analitic_rjl) || (rjb.id_CA_creditor == id_cont_analitic_rjl))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();
             if ((luna_rjl != 0) && (id_cont_analitic_rjl != 0))
             rj_5121 =await _context.registru_jurnal.Where(rjb => (rjb.data.Month==luna_rjl) && (rjb.data.Year==DateTime.Now.Year) && ((rjb.id_CA_debitor == id_cont_analitic_rjl) || (rjb.id_CA_creditor == id_cont_analitic_rjl))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();
             if ((luna_rjl == 13) && (id_cont_analitic_rjl != 0))
             rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Year == DateTime.Now.Year) && ((rjb.id_CA_debitor == id_cont_analitic_rjl) || (rjb.id_CA_creditor == id_cont_analitic_rjl))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();

             }

             if (nr_document != "0")
             {

            // if ((nr_document != "5") && (nr_document != "4") && (nr_document != "3"))
           //  {
             if (luna_rjl == 0)
             rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Day == ziua_rjl.Day) && (rjb.data.Month == ziua_rjl.Month) && (rjb.data.Year == DateTime.Now.Year) && ((rjb.nr_document  == nr_document))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();

             if (luna_rjl != 0)
              rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Month == luna_rjl) && (rjb.data.Year == DateTime.Now.Year) && ((rjb.nr_document == nr_document))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();

             // }
             

             //if ((nr_document == "5") || (nr_document == "4") || (nr_document == "3"))
             //{
             //if (luna_rjl == 0)
             //rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Day == ziua_rjl.Day) && (rjb.data.Month == ziua_rjl.Month) && (rjb.data.Year == DateTime.Now.Year) 
               //          && (rjb.id_CS_debitor != 53) && (rjb.id_CS_creditor != 53) && (rjb.id_CS_debitor != 107) && (rjb.id_CS_debitor != 106) &&
                 //       ((rjb.nr_document  == nr_document))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();

             //if (luna_rjl != 0)
             // rj_5121 = await _context.registru_jurnal.Where(rjb => (rjb.data.Month == luna_rjl) && (rjb.data.Year == DateTime.Now.Year)
              // && (rjb.id_CS_debitor != 53) && (rjb.id_CS_creditor != 53) && (rjb.id_CS_debitor != 107) && (rjb.id_CS_debitor != 106) 
              //&& ((rjb.nr_document == nr_document))).Include(rjb => rjb.conturi_sintetice_C).Include(rjb => rjb.conturi_sintetice_D).Include(rjb => rjb.conturi_analitice_C).Include(rjb => rjb.conturi_analitice_D).OrderBy(rjb => rjb.data).ThenBy(rjb => rjb.id_document).ToListAsync();

              //}
             

             }


             if (nr_document == "0")
             {
             incastlfc = rj_5121.Where(rjc => rjc.id_CS_creditor == id_cont_sintetic_rjl).Sum(rjc => rjc.credit);
             platatlfc = rj_5121.Where(rjc => rjc.id_CS_debitor == id_cont_sintetic_rjl).Sum(rjc => rjc.credit);
             if(activlfc) SFlfc = silfc + platatlfc - incastlfc;
             if(!activlfc) SFlfc = silfc - platatlfc + incastlfc;
             }

             if (nr_document != "0")
             {
             incastlfc = rj_5121.Sum(rjc => rjc.credit);
             platatlfc = incastlfc;
             }

             Cell datalfc_titlu = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("data")).SetBold().AddStyle(normalTblHead2);

             Cell nrlfc_titlu = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("nr. document")).SetBold().AddStyle(normalTblHead2);

             Cell CDlfc_titlu = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("cont debit")).SetBold().AddStyle(normalTblHead2);

             Cell explCDlfc_titlu = new Cell(1, 1)
                 .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                 .Add(new Paragraph("explicatie cont debit")).SetBold().AddStyle(normalTblHead2);

             Cell CClfc_titlu = new Cell(1, 1)
                 .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                 .Add(new Paragraph("cont credit")).SetBold().AddStyle(normalTblHead2);

             Cell explCClfc_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("explicatie cont credit")).SetBold().AddStyle(normalTblHead2);

             Cell creditlfc_titlu = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("credit")).SetBold().AddStyle(normalTblHead2);

             Cell debitlfc_titlu = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("debit")).SetBold().AddStyle(normalTblHead2);
                            
            table.AddHeaderCell(datalfc_titlu);
            table.AddHeaderCell(nrlfc_titlu);
            table.AddHeaderCell(CDlfc_titlu);
            table.AddHeaderCell(explCDlfc_titlu);
            table.AddHeaderCell(CClfc_titlu);
            table.AddHeaderCell(explCClfc_titlu);
            table.AddHeaderCell(creditlfc_titlu);
            table.AddHeaderCell(debitlfc_titlu);
                
                
            foreach (registru_jurnal rjb in rj_5121 )
             {


              Cell datalfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(rjb.data.ToString("dd.MM.yyyy"))).AddStyle(normalTbl2);

              Cell nrlfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(rjb.nr_document)).AddStyle(normalTbl2);            
              nrcontD = "";
              explContD = "";
             
              nrcontD = rjb.conturi_sintetice_D.nr_cont_sintetic.ToString();
              explContD = rjb.conturi_sintetice_D.explicatie_nr_cont_sintetic;              
              if (rjb.conturi_analitice_D.nr_cont_analitic != "-")
                    {
                      //  CAListFC = await _context.conturi_analitice.Where(calfc => calfc.id_cont_sintetic == rjb.id_CS_debitor).ToListAsync();
                        nrcontD = rjb.conturi_analitice_D.nr_cont_analitic;
                        explContD = rjb.conturi_analitice_D.explicatie_nr_cont_analitic;
                    }
               Cell nrcsdlfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(nrcontD)).AddStyle(normalTbl2);
               Cell explcsdlfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(explContD)).AddStyle(normalTbl2);

              nrcontC = "";
              explContC = "";
              //CAListFC = await _context.conturi_analitice.Where(calfc => calfc.id_cont_sintetic == rjb.id_CS_creditor).SingleOrDefaultAsync();          
              nrcontC = rjb.conturi_sintetice_C.nr_cont_sintetic.ToString();
              explContC = rjb.conturi_sintetice_C.explicatie_nr_cont_sintetic;              
              if (rjb.conturi_analitice_C.nr_cont_analitic != "-")
                    {
                        nrcontC = rjb.conturi_analitice_C.nr_cont_analitic;
                        explContC = rjb.conturi_analitice_C.explicatie_nr_cont_analitic;
                    }
               Cell nrcsclfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(nrcontC)).AddStyle(normalTbl2);
               Cell explcsclfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(explContC)).AddStyle(normalTbl2);


              Cell creditlfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(rjb.credit.ToString("N2"))).AddStyle(normalTbl2);

              Cell debitlfc = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(rjb.debit.ToString("N2"))).AddStyle(normalTbl2);

              table.AddCell(datalfc);
              table.AddCell(nrlfc);
              table.AddCell(nrcsdlfc);
              table.AddCell(explcsdlfc);
              table.AddCell(nrcsclfc);
              table.AddCell(explcsclfc);
              table.AddCell(creditlfc);
              table.AddCell(debitlfc);

             }

            }
            //pa code  mai

            if ((nr_document== "0") && (testCDA==1))
            if ((id_cont_sintetic_rjl !=18) && (id_cont_sintetic_rjl != 26) && (id_cont_sintetic_rjl != 27) && (id_cont_sintetic_rjl != 52))
            {
                ViewData["Error"] = "Nu se pot face deocamdata listari decat pentru conturile 4611, 4614, 4621";
              //  return View("Error");
            }

            DateTime dataCHnrlow = Convert.ToDateTime(DateTime.Now.Year.ToString() + "-1-12 00:00:00");
                    

            var pensFC =await _context.Pensionars.Where(pfc => pfc.sold_462_2016 != 0).OrderBy(pfc => pfc.nrcarnet).ToListAsync();
            if (id_cont_sintetic_rjl==26) pensFC = await _context.Pensionars.Where(pfc => (((pfc.sold_461 != 0) && (pfc.sold_461_2016 == 0)) || (pfc.sold_461_2016 != 0)) && (pfc.id_stare==3)).OrderBy(pfc => pfc.nrcarnet).ToListAsync();
            if (id_cont_sintetic_rjl == 27) pensFC = await _context.Pensionars.Where(pfc => (((pfc.sold_461 != 0) && (pfc.sold_461_2016==0)) || (pfc.sold_461_2016 != 0)) && (pfc.id_stare == 4)).OrderBy(pfc => pfc.nrcarnet).ToListAsync();
            if (id_cont_sintetic_rjl == 18) pensFC = await _context.Pensionars.Where(pfc => (pfc.sold_462_2016 != 0)).OrderBy(pfc => pfc.nrcarnet).ToListAsync();
            if (nr_document!= "0") pensFC = await _context.Pensionars.Where(pfc => (pfc.nrcarnet==1)).OrderBy(pfc => pfc.nrcarnet).ToListAsync();
                        
            string nrContCA = "4621" ;        
                      

            int idcsAddcell = 0;                            
                    
            int i = 0;

            //if (id_cont_sintetic_rjl!=52)
            //wwif ((testCDA==1) || (nr_document != "0"))
            if ((testCDA == 1) && (nr_document == "0"))
            foreach ( Pensionar pfc in pensFC)
            {

                if (nr_document == "0")  
                {
                
                if (id_cont_sintetic_rjl==18) nrContCA = "4621." + pfc.nrcarnet.ToString();                

                CAListFC = await _context.conturi_analitice.Where(calfc => calfc.nr_cont_analitic == nrContCA).SingleOrDefaultAsync();
                if (id_cont_sintetic_rjl == 26) CAListFC = await _context.conturi_analitice.Where(calfc => calfc.id_cont_analitic == pfc.nrcarnet).SingleOrDefaultAsync();
                if (id_cont_sintetic_rjl == 27) CAListFC = await _context.conturi_analitice.Where(calfc => calfc.id_cont_analitic == pfc.nrcarnet).SingleOrDefaultAsync();

              
                if ((id_cont_sintetic_rjl == 18)  || (id_cont_sintetic_rjl == 26) || (id_cont_sintetic_rjl == 26))
                {
                 nrContCA = CAListFC.nr_cont_analitic;
                 explCA= CAListFC.explicatie_nr_cont_analitic;
                 }
                                  
                if (id_cont_sintetic_rjl == 27) nrContCA = CAListFC.nr_cont_analitic;

                if (luna_rjl !=0 )
                rjListFC = await _context.registru_jurnal.Where(rjlfc => (rjlfc.data.Month==luna_rjl) && ((rjlfc.id_CA_debitor == CAListFC.id_cont_analitic) || (rjlfc.id_CA_creditor == CAListFC.id_cont_analitic))).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D).Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).OrderBy(rjlfc=>rjlfc.data).ToListAsync();

                if (luna_rjl == 13)
                rjListFC = await _context.registru_jurnal.Where(rjlfc => (rjlfc.data.Year == DateTime.Now.Year) && ((rjlfc.id_CA_debitor == CAListFC.id_cont_analitic) || (rjlfc.id_CA_creditor == CAListFC.id_cont_analitic))).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D).Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).OrderBy(rjlfc => rjlfc.data).ToListAsync();

                 if (luna_rjl == 0)
                 rjListFC = await _context.registru_jurnal.Where(rjlfc => (rjlfc.data.Month ==ziua_rjl.Month) && (rjlfc.data.Day==ziua_rjl.Day) && ((rjlfc.id_CA_debitor == CAListFC.id_cont_analitic) || (rjlfc.id_CA_creditor == CAListFC.id_cont_analitic))).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D).Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).OrderBy(rjlfc => rjlfc.data).ToListAsync();

                }                  


              //  if ((nr_document != "0") && (luna_rjl==0) && (nr_document != "10") && (nr_document != "6") && (nr_document != "7"))
                //    rjListFC = await _context.registru_jurnal.Where(rjlfc => (rjlfc.data.Month == ziua_rjl.Month) && (rjlfc.data.Day == ziua_rjl.Day) && (rjlfc.nr_document == nr_document) && (rjlfc.id_CS_debitor != 53) && (rjlfc.id_CS_creditor != 53) && (rjlfc.id_CS_debitor!=107) && (rjlfc.id_CS_debitor!=106) && (rjlfc.data>dataCHnrlow)).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D).Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).OrderBy(rjlfc => rjlfc.data).ThenBy(rjlfc=>rjlfc.id_document).ToListAsync();

               // if ((nr_document != "0") && (luna_rjl != 0) && (nr_document != "10") && (nr_document != "6") && (nr_document != "7"))
                 //       rjListFC = await _context.registru_jurnal.Where(rjlfc => (rjlfc.data.Month == luna_rjl ) && (rjlfc.nr_document == nr_document) && (rjlfc.id_CS_debitor != 53) && (rjlfc.id_CS_creditor != 53) && (rjlfc.id_CS_debitor != 107) && (rjlfc.id_CS_debitor != 106) && (rjlfc.data>dataCHnrlow)).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D).Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).OrderBy(rjlfc => rjlfc.data).OrderBy(rjlfc => rjlfc.data).ThenBy(rjlfc => rjlfc.id_document).ToListAsync();
                 
               // if ((nr_document== "10") || (nr_document == "6") || (nr_document == "7"))
                 // if (luna_rjl!=0)
                   // rjListFC = await _context.registru_jurnal.Where(rjlfc => (rjlfc.data.Month == luna_rjl) && (rjlfc.data.Year == DateTime.Now.Year) && (rjlfc.nr_document == nr_document) &&  (rjlfc.data>dataCHnrlow)).Include(r => r.conturi_sintetice_C).Include(r => r.conturi_sintetice_D).Include(r => r.conturi_analitice_C).Include(r => r.conturi_analitice_D).OrderBy(rjlfc => rjlfc.data).ThenBy(rjlfc=>rjlfc.id_document).ToListAsync();


                if (nr_document != "0") nrContCA = "";

                incastlfc = rjListFC.Sum(rjlfc => rjlfc.credit);
                platatlfc = rjListFC.Sum(rjlfc => rjlfc.credit);


                colhh4P = "";
                colhh5P = "";
                colhh6P = "";
                colhh7P = "";
                if ((rjListFC.Count() == 0) && (nr_document == "0"))
                {
                    colhh4P = "sold initial";
                    colhh6P = "sold final";
                    soldCD = 0;
                    if (id_cont_sintetic_rjl == 18) soldCD = pfc.sold_462_2016;
                    if (id_cont_sintetic_rjl == 26) soldCD = pfc.sold_461_2016;
                    if (id_cont_sintetic_rjl == 27) soldCD = pfc.sold_461_2016;
                    colhh5P = soldCD.ToString("N2");
                    colhh7P = soldCD.ToString("N2");
                }
                 Cell nrd_titlu = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(nrContCA)).SetBold().AddStyle(normalTblHead);

                SFSI18 = explCA;               
                Cell colhh2 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph(SFSI18)).AddStyle(normalTblHead);


                    Cell colhh31 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("")).SetBold().AddStyle(normalTblHead);

                    Cell colhh3 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("")).SetBold().AddStyle(normalTblHead);

                Cell colhh4 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph(colhh4P)).AddStyle(normalTblHead);

                Cell colhh5 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph(colhh5P)).AddStyle(normalTblHead);

                Cell colhh6 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                   .Add(new Paragraph(colhh6P)).AddStyle(normalTblHead);

                Cell colhh7 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph(colhh7P)).AddStyle(normalTblHead);

                

                if (nr_document == "0")
                {
                        table.AddCell(nrd_titlu);
                        table.AddCell(colhh2);
                        table.AddCell(colhh3);
                        table.AddCell(colhh31);
                        table.AddCell(colhh4);
                        table.AddCell(colhh5);
                        table.AddCell(colhh6);
                        table.AddCell(colhh7);
                }                     
            
                Cell colhhh1 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("nr. document")).AddStyle(normalTbl);

                Cell colhhh2 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("data")).AddStyle(normalTbl);

                Cell colhhh3 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("cont debitor")).AddStyle(normalTbl);               

                Cell colhhh4 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("cont creditor")).AddStyle(normalTbl);

                Cell colhhh41 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("explicatie")).AddStyle(normalTbl);

                    Cell colhhh5 = new Cell(1, 1)
              .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                 .Add(new Paragraph("suma")).AddStyle(normalTbl);

                    Cell colhhh6 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                  .Add(new Paragraph("sold initial")).AddStyle(normalTbl);

                Cell colhhh7 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                   .Add(new Paragraph("sold final")).AddStyle(normalTbl);

                        

           //   if (nr_document == "0") table.AddCell(colhhh1);

           //ww  if (nr_document != "0")
               //ww{
               //wwtable.AddHeaderCell(colhhh2);
               //wwtable.AddHeaderCell(colhhh3);
               //wwtable.AddHeaderCell(colhhh4);
               //wwtable.AddHeaderCell(colhhh5);
              //ww }

             if ((nr_document == "0") && (rjListFC.Count() != 0))
               {
                table.AddCell(colhhh1);
                table.AddCell(colhhh2);
                table.AddCell(colhhh3);
                table.AddCell(colhhh4);
                table.AddCell(colhhh41);
                table.AddCell(colhhh5);
                table.AddCell(colhhh6);
                table.AddCell(colhhh7);
                }
                
             //   if (nr_document == "0")table.AddCell(colhhh6);
               // if (nr_document == "0")table.AddCell(colhhh7);


             
                i = 0;
                foreach (registru_jurnal rjlfc in rjListFC)
                {
                    
                i = i + 1;

                nrcontC = "";
                nrcontD = "";
                nrcontD = rjlfc.conturi_analitice_D.nr_cont_analitic;
                if (rjlfc.conturi_analitice_D.explicatie_nr_cont_analitic == "nu are conturi analitice") nrcontD = rjlfc.conturi_sintetice_D.nr_cont_sintetic.ToString();
                if (rjlfc.conturi_analitice_D.nr_cont_analitic == "-") nrcontD = rjlfc.conturi_sintetice_D.nr_cont_sintetic.ToString();
                nrcontC = rjlfc.conturi_analitice_C.nr_cont_analitic;
                if (rjlfc.conturi_analitice_C.explicatie_nr_cont_analitic == "nu are conturi analitice") nrcontC = rjlfc.conturi_sintetice_C.nr_cont_sintetic.ToString();
                if (rjlfc.conturi_analitice_C.nr_cont_analitic == "-") nrcontC = rjlfc.conturi_sintetice_C.nr_cont_sintetic.ToString();
                Cell data = new Cell(1, 1)
                                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                      .Add(new Paragraph(rjlfc.data.ToString("dd.MM.yyyy"))).AddStyle(normalTbl);
                 
                
                 Cell nrd = new Cell(1, 1)
                                   .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                     .Add(new Paragraph(rjlfc.nr_document)).AddStyle(normalTbl);

                 Cell ca_d = new Cell(1, 1)
                                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                      .Add(new Paragraph(nrcontD)).AddStyle(normalTbl);

                 Cell si_ca_d = new Cell(1, 1)
                                    .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                     .Add(new Paragraph(rjlfc.SI_CA_debit.ToString("N2"))).AddStyle(normalTbl);

                Cell sf_ca_d = new Cell(1, 1)
                                    .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                     .Add(new Paragraph(rjlfc.SF_CA_debit.ToString("N2"))).AddStyle(normalTbl);

              
                Cell ca_c = new Cell(1, 1)
                                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                    .Add(new Paragraph(nrcontC)).AddStyle(normalTbl);

                Cell ca_e = new Cell(1, 1)
                                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                   .Add(new Paragraph(rjlfc.explicatie_nr_cont_analitic)).AddStyle(normalTbl);


               Cell si_ca_c = new Cell(1, 1)
                                   .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                   .Add(new Paragraph(rjlfc.SI_CA_credit.ToString("N2"))).AddStyle(normalTbl);

                Cell sf_ca_c = new Cell(1, 1)
                                  .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                   .Add(new Paragraph( rjlfc.SF_CA_credit.ToString("N2"))).AddStyle(normalTbl);
                Cell suma = new Cell(1, 1)
                                  .SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                  .Add(new Paragraph(rjlfc.credit.ToString("N2"))).AddStyle(normalTbl);

                if (nr_document == "0") table.AddCell(nrd);
                table.AddCell(data);

                table.AddCell(ca_d);
                
                table.AddCell(ca_c);

                 if (nr_document == "0")  table.AddCell(ca_e);    

                table.AddCell(suma);
                         
                if ((nr_document == "0") && ((rjlfc.id_CS_debitor==id_cont_sintetic_rjl) )) table.AddCell(si_ca_d);
                if ((nr_document == "0") && ((rjlfc.id_CS_creditor == id_cont_sintetic_rjl))) table.AddCell(si_ca_c);
                 if ((nr_document == "0") && ((rjlfc.id_CS_debitor==id_cont_sintetic_rjl) )) table.AddCell(sf_ca_d);
                if ((nr_document == "0") && ((rjlfc.id_CS_creditor == id_cont_sintetic_rjl))) table.AddCell(sf_ca_c);

                }

            

            }
                     

            //
            Paragraph ptsilfc = new Paragraph();
            ptsilfc.Add(new Text("Sold intial ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptsilfc.Add(new Text(silfc.ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));

            Paragraph ptcdlfc = new Paragraph();
            ptcdlfc.Add(new Text("Total credit ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptcdlfc.Add(new Text(incastlfc.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));
            ptcdlfc.Add(new Text("Total debit ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal).SetBold());
            ptcdlfc.Add(new Text(platatlfc.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal));

            Paragraph ptsflfc = new Paragraph();
            ptsflfc.Add(new Text("Sold final ").SetTextAlignment(TextAlignment.LEFT)
           .SetFontSize(13).AddStyle(normal).SetBold());
            ptsflfc.Add(new Text(SFlfc.ToString
                ("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));



            //if 
            if ((testCDA == 0) && (nr_document == "0")) document.Add(ptsilfc);
            //if (id_cont_sintetic_rjl==52) document.Add(ptsilfc);
            if ((id_cont_sintetic_rjl != 18) && (id_cont_sintetic_rjl != 26) && (id_cont_sintetic_rjl != 27))  document.Add(ptcdlfc);
            //if (id_cont_sintetic_rjl == 52) document.Add(ptsflfc);
            if ((testCDA == 0) && (nr_document == "0")) document.Add(ptsflfc);
            document.Add(new Paragraph(" "));

            //


            document.Add(table);
            table.Complete();


            i = 1;
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y - 20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }

          
            titlu1_raport_rj =titlu;
            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);


            i = 1;
            int numberOfPages = pdfDoc.GetNumberOfPages();
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float y = 20f;
                float x = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy") + "                                                                                                                             " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);

            }

            document.Close();
            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");
            // using (FileStream fs = File.Create("C:\\test.pdf")) { fs.Write(byteInfo, 0, (int)byteInfo.Length); } 

            //File(byteInfo, "application/pdf", "E:\\JOB\\test.pdf");
            return fileStreamResult;

            
        }


        [HttpGet]
        public ActionResult introducere_nr_cont_analitic()
        {

            var ContSinteticList = _context.conturi_sintetice
                                       .Where(cs => (cs.id_cont_sintetic ==18) || (cs.id_cont_sintetic == 1)).Select(cs =>
                           new SelectListItem
                           {

                               Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                               Value = cs.id_cont_sintetic.ToString()
                           });

            ViewData["id_cont_sintetic"] = new SelectList(ContSinteticList, "Value", "Text");

            return View();

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult introducere_nr_cont_analitic([Bind("id_cont_analitic,id_cont_sintetic,nr_cont_analitic,explicatie_nr_cont_analitic")] conturi_analitice conturi_analitice)
        {
            int ids;
            int nrcarnetIns18 = 0;

            if (conturi_analitice.nr_cont_analitic.Length < 4)
            {
                ViewData["Error"] = "Ati introdus mai putin de 4 caractere la nr cont analitic";
                return View("Error");
            }

            if (conturi_analitice.id_cont_sintetic == 0)

            {
                ViewData["Error"] = "Introduceti cont sintetic";
                return View("Error");
            }

            if ((conturi_analitice.id_cont_sintetic != 1) && (conturi_analitice.id_cont_sintetic != 18))
            {
                ViewData["Error"] = "Nu se pot introduce conturi analitice decat pentru furnizori si creditori";
                return View("Error");
            }

            if ((conturi_analitice.nr_cont_analitic.Substring(0, 4) != "401.") && (conturi_analitice.nr_cont_analitic.Substring(0, 4) != "462.") && (conturi_analitice.nr_cont_analitic.Substring(0, 5) != "4621."))

            {
                ViewData["Error"] = "Nr cont analitic incorect";
                return View("Error");
            }

            var pens18SI1 = _context.Pensionars.Where(psi18 => psi18.nrcarnet == 1).SingleOrDefault();

            var cekNrCA = _context.conturi_analitice.Where(cnca => cnca.nr_cont_analitic == conturi_analitice.nr_cont_analitic.Trim());
            if (cekNrCA.Count() > 0)
            {
                ViewData["Error"] = "Acest nr cont analitic mai exista";
                return View("Error");
            }

            int idcontA = 0;

            int getID_contA = _context.conturi_analitice
                .Where(girc => (girc.id_cont_analitic < 500) && (girc.id_cont_analitic > 300)).Max(girc => girc.id_cont_analitic);

            int getID_contA_4621 = _context.conturi_analitice
           .Where(girc462 => (girc462.id_cont_sintetic == 18) && (girc462.id_cont_analitic < 300)).Max(girc462 => girc462.id_cont_analitic);

            idcontA = getID_contA;

            if (conturi_analitice.id_cont_sintetic == 18) idcontA = getID_contA_4621;

            ModelState.Remove("conturi_sintetice_C");

            if (ModelState.IsValid)
            {

                _context.conturi_analitice.Add(new conturi_analitice
                {
                    //id_cont_sintetic = Int32.Parse(id_cont_sintetic),
                    id_cont_sintetic = conturi_analitice.id_cont_sintetic,
                    id_cont_analitic = idcontA + 1,
                    nr_cont_analitic = conturi_analitice.nr_cont_analitic.Trim(),
                    explicatie_nr_cont_analitic = conturi_analitice.explicatie_nr_cont_analitic.Trim()
                });

                _context.SaveChanges();

                //  
                // 
                if (conturi_analitice.id_cont_sintetic == 18)
                {
                    nrcarnetIns18 = 0;
                    nrcarnetIns18 = Int32.Parse(conturi_analitice.nr_cont_analitic.Remove(0, 5));
                    pens18SI1 = _context.Pensionars.Where(psi18 => psi18.nrcarnet == nrcarnetIns18).SingleOrDefault();
                    if (pens18SI1 != null)
                    {
                        pens18SI1.sold_462_2016 = 1;
                        _context.SaveChanges();
                    }

                }
                //  _context.Pensionars.Where()

                return RedirectToAction("Create");
            }

            var ContSinteticList = _context.conturi_sintetice
                                      .Where(cs => (cs.id_cont_sintetic == 18) || (cs.id_cont_sintetic == 1)).Select(cs =>
                         new SelectListItem
                         {

                             Text = cs.nr_cont_sintetic.ToString() + " " + cs.explicatie_nr_cont_sintetic.ToString(),
                             Value = cs.id_cont_sintetic.ToString()
                         });

            ViewData["id_cont_sintetic"] = new SelectList(ContSinteticList, "Value", "Text");

            return View();
        }

    }
}
