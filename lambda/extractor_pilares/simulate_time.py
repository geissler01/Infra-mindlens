import os
import sys
from datetime import date, timedelta

# Asegurar path para imports relativos si se corre directo
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from app import lambda_handler

def main():
    print("=====================================================")
    print("    SIMULADOR DE TIEMPO - EXTRACCIÓN DE PILARES      ")
    print("=====================================================")
    
    # Rango de fechas de los diarios generados (del 14 al 29 de junio de 2026)
    start_date = date(2026, 6, 14)
    end_date = date(2026, 6, 29)
    
    delta = timedelta(days=1)
    current_date = start_date
    
    total_dias = (end_date - start_date).days + 1
    dia_actual = 1
    
    while current_date <= end_date:
        fecha_str = current_date.strftime("%Y-%m-%d")
        print("\n" + "="*50)
        print(f"⏩ VIAJANDO EN EL TIEMPO: DÍA {dia_actual}/{total_dias} -> {fecha_str}")
        print("="*50)
        
        # Simular el payload de EventBridge con la fecha target
        event = {
            "target_date": fecha_str
        }
        
        # Ejecutar la lambda de forma síncrona
        lambda_handler(event, None)
        
        current_date += delta
        dia_actual += 1
        
    print("\n=====================================================")
    print(" SIMULACIÓN COMPLETADA CON ÉXITO")
    print(" La tabla 'journaling_register' está ahora poblada.")
    print("=====================================================")

if __name__ == "__main__":
    main()
