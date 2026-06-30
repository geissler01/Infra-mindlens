import os
import sys

# Asegurar path para imports relativos
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from app import lambda_handler

def main():
    print("==========================================================")
    print("    SIMULADOR SEMANAL - GENERADOR DE REPORTES (CLUSTERS)  ")
    print("==========================================================")
    
    # Fechas objetivo para simular la generacion de reportes dominicales
    # Nuestra data simulada va del 14 al 29 de junio de 2026.
    target_dates = ["2026-06-21", "2026-06-28"]
    
    for fecha_str in target_dates:
        print("\n" + "="*50)
        print(f"⏩ VIAJANDO EN EL TIEMPO: SEMANA TERMINADA EN -> {fecha_str}")
        print("="*50)
        
        event = {
            "target_date": fecha_str
        }
        
        # Ejecutar la lambda de forma sincrona
        lambda_handler(event, None)
        
    print("\n==========================================================")
    print(" SIMULACION SEMANAL COMPLETADA CON EXITO")
    print(" Las tablas 'weekly_reports' y 'weekly_cluster_reports'")
    print(" estan ahora pobladas.")
    print("==========================================================")

if __name__ == "__main__":
    main()
